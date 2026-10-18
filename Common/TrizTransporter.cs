namespace TrizDB
{
	using System;
	using System.IO;
	using System.Text;
	using System.VarInt;
	using DataContainer = System.Collections.Generic.List<System.Tuple<Names, object>>;
	using DataItem = System.Tuple<Names, object>;

	public static class TrizTransporter
	{
		public static void Write(BinaryWriter stream, DataContainer items)
		{
			VarUInt32.EncodeToBinaryStream(stream, (uint)items.Count);
			foreach (var item in items)
			{
				VarUInt32.EncodeToBinaryStream(stream, (uint)item.Item1);
				GetFuncWriteDataContent(item.Item1)(stream, item.Item2);
			}
		}

		public static DataContainer Read(BinaryReader stream)
		{
			var result = new DataContainer();
			uint count = VarUInt32.DecodeFromStream(stream);
			for (uint i = 0; i < count; i++)
			{
				var type = VarUInt32.DecodeFromStream(stream);
				var data = GetFuncReadDataContent(stream, (Names)type)(stream);
				result.Add(new DataItem((Names)type, data));
			}
			return result;
		}

		private static Func<BinaryReader, object> GetFuncReadDataContent(BinaryReader stream, Names type)
		{
			switch (type)
			{
				case Names.QRCode:
				case Names.InventoryNumber:
				case Names.AssetsName: return ReadText;
				case Names.Photo: return ReadBytes;
			}
			throw new NotImplementedException();
		}

		private static Action<BinaryWriter, object> GetFuncWriteDataContent(Names type)
		{
			switch (type)
			{
				case Names.QRCode:
				case Names.InventoryNumber:
				case Names.AssetsName: return WriteText;
				case Names.Photo: return WriteBytes;
			}
			throw new NotImplementedException();
		}

		private static object ReadText(BinaryReader stream)
		{
			int count = (int)VarUInt32.DecodeFromStream(stream);
			byte[] data = stream.ReadBytes(count);
			return Encoding.UTF8.GetString(data);
		}

		private static void WriteText(BinaryWriter stream, object value)
		{
			byte[] data = Encoding.UTF8.GetBytes((string)value);
			VarUInt32.EncodeToBinaryStream(stream, (uint)data.Length);
			stream.Write(data);
		}

		private static object ReadNumber(BinaryReader stream)
		{
			return VarUInt32.DecodeFromStream(stream);
		}

		private static void WriteNumber(BinaryWriter stream, object value)
		{
			VarUInt32.EncodeToBinaryStream(stream, (uint)value);
		}

		private static object ReadBytes(BinaryReader stream)
		{
			int count = (int)VarUInt32.DecodeFromStream(stream);
			return stream.ReadBytes(count);
		}

		private static void WriteBytes(BinaryWriter stream, object value)
		{
			byte[] data = (byte[])value;
			VarUInt32.EncodeToBinaryStream(stream, (uint)data.Length);
			stream.Write(data);
		}
	}
}