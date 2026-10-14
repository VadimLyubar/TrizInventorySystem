using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace FirstASP
{
	public class SocketService
	{
		private string _ip;
		private int _port;
		private readonly object _lock = new object();

		public SocketService(string ip, int port) { _ip = ip; _port = port; }

		public void StartBackgroundConnection() { } // Метод пуст, так как сокет транзакционный

		public string SendStringCommand(string cmd)
		{
			lock (_lock)
			{
				try
				{
					using (TcpClient client = new TcpClient(_ip, _port))
					{
						NetworkStream s = client.GetStream();
						BinaryWriter w = new BinaryWriter(s);
						BinaryReader r = new BinaryReader(s);

						byte[] cBytes = Encoding.UTF8.GetBytes(cmd);
						w.Write(cBytes.Length);
						w.Write(cBytes);
						w.Flush();

						int rLen = r.ReadInt32();
						return Encoding.UTF8.GetString(r.ReadBytes(rLen));
					}
				}
				catch (Exception ex) { return "ERR_NETWORK: " + ex.Message; }
			}
		}

		public string SendAssetPayload(string qr, string inv, string name, string loc, string usr, string molId, List<byte[]> photos)
		{
			lock (_lock)
			{
				try
				{
					using (TcpClient client = new TcpClient(_ip, _port))
					{
						NetworkStream s = client.GetStream();
						BinaryWriter w = new BinaryWriter(s);
						BinaryReader r = new BinaryReader(s);

						byte[] headBytes = Encoding.UTF8.GetBytes("ADD_ASSET");
						w.Write(headBytes.Length); w.Write(headBytes);

						byte[] textBytes = Encoding.UTF8.GetBytes(string.Format("{0}|{1}|{2}|{3}|{4}|{5}", qr, inv, name, loc, usr, molId));
						w.Write(textBytes.Length); w.Write(textBytes);

						w.Write(photos.Count);
						foreach (byte[] photo in photos)
						{
							w.Write(photo.Length);
							w.Write(photo);
						}
						w.Flush();

						int rLen = r.ReadInt32();
						return Encoding.UTF8.GetString(r.ReadBytes(rLen));
					}
				}
				catch (Exception ex) { return "Сбой передачи бинарного пакета: " + ex.Message; }
			}
		}

		public List<Tuple<int, string>> DownloadMOLList()
		{
			List<Tuple<int, string>> list = new List<Tuple<int, string>>();
			lock (_lock)
			{
				try
				{
					using (TcpClient client = new TcpClient(_ip, _port))
					{
						NetworkStream s = client.GetStream();
						BinaryWriter w = new BinaryWriter(s);
						BinaryReader r = new BinaryReader(s);

						byte[] cmd = Encoding.UTF8.GetBytes("GET_MOL_LIST");
						w.Write(cmd.Length); w.Write(cmd); w.Flush();

						int count = r.ReadInt32();
						for (int i = 0; i < count; i++)
						{
							int id = r.ReadInt32();
							int nameLen = r.ReadInt32();
							string fioText = Encoding.UTF8.GetString(r.ReadBytes(nameLen));
							list.Add(new Tuple<int, string>(id, fioText));
						}
					}
				}
				catch { }
			}
			return list;
		}
	}
}