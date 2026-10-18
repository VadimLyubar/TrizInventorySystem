namespace System.VarInt
{
	using System;
	using System.IO;

	public static class VarUInt32
	{
		private const int max_shift = 32;


		public static uint DecodeFromStream(BinaryReader stream)
		{
			int shift = 0;
			uint result = 0;
			bool is_continue;
			do
			{
				if (shift > max_shift) throw new OverflowException();
				byte b = stream.ReadByte();
				is_continue = (b & 0x80) > 0;
				result |= (uint)((b & 0x7F) << shift);
				shift += 7;
			} while (is_continue);
			return result;
		}


		public static void EncodeToBinaryStream(BinaryWriter stream, uint value)
		{
			while (value > 0x7F)
			{
				stream.Write((byte)(value | 0x80));
				value >>= 7;
			}
			stream.Write((byte)value);
		}
	}
}