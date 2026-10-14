using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;


namespace TrizDB
{
	partial class TrizDBService
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.ServiceName = "TrizDB";
		}
	}
}


namespace TrizDB
{
	public partial class TrizDBService : ServiceBase
	{
		private TcpListener _listener;
		private Thread _listenerThread;
		private bool _isRunning;

		// ПОТОКОБЕЗОПАСНЫЙ СВЕРНУТЫЙ СЧЕТЧИК УНИКАЛЬНЫХ КЛЮЧЕЙ СУЩНОСТЕЙ
		private static int _globalSetIdCounter = 0;

		// ПЕРЕЧИСЛЕНИЕ СУЩНОСТЕЙ (АТОМАРНЫЙ ГРАФ СВЯЗЕЙ В ОДНОМ СПИСКЕ ПО СЫЧЁВУ)
		public enum Names
		{
			// Свойства Объекта Материального Имущества (ОМИ)
			QRCode = 1,
			InventoryNumber = 2,
			AssetsName = 3,
			Photo = 4,
			ResponsibleAssets = 5,      // Ссылка на Set_id сущности МОЛ из этого же списка
			AssetLocationID = 6,        // Ссылка на Set_id сущности Местоположения из этого же списка
			AssetUsers = 7,

			// Свойства Материально Ответственного Лица (МОЛ)
			LastName = 8,
			FirstName = 9,
			MiddleName = 10,
			ResponsibleAssetsPosition = 11, // Главный маркер-определитель пачки МОЛ

			// Свойства Сущности Местоположения
			AssetLocationText = 12,
			LocationDateTime = 13
		}

		public class SetsRecord
		{
			public Int32 Set_id { get; set; }
			public Names Name_id { get; set; }
			public Int32 Value_id { get; set; }
		}

		public class ValuesRecord
		{
			public Int32 Value_id { get; set; }
			public string Value { get; set; }
		}

		public static List<SetsRecord> SetsTable = new List<SetsRecord>();
		public static List<ValuesRecord> ValuesTable = new List<ValuesRecord>();

		private static readonly object _dbLock = new object();
		private string _basePath;
		private string _photosPath;

		public TrizDBService()
		{
			InitializeComponent();
			_basePath = AppDomain.CurrentDomain.BaseDirectory;
			_photosPath = Path.Combine(_basePath, "PhotosStorage");
		}

		// МЕТОД ПОЛУЧЕНИЯ СЛЕДУЮЩЕГО СВОБОДНОГО КЛЮЧА (ГАРАНТИЯ ОТ НАЛОЖЕНИЯ ДО ЗАПИСИ)
		private static int GetNextUniqueSetId()
		{
			return Interlocked.Increment(ref _globalSetIdCounter);
		}

		protected override void OnStart(string[] args)
		{
			if (!Directory.Exists(_photosPath)) Directory.CreateDirectory(_photosPath);

			LoadDatabaseFromDisk();

			// Инициализируем сквозной генератор максимальным ID из базы + 1
			int maxId = 0;
			foreach (var s in SetsTable)
			{
				if (s.Set_id > maxId) maxId = s.Set_id;
			}
			_globalSetIdCounter = maxId;

			_isRunning = true;
			_listener = new TcpListener(IPAddress.Any, 65525);
			_listener.Start();

			_listenerThread = new Thread(ListenForClients);
			_listenerThread.IsBackground = true;
			_listenerThread.Start();
		}

		private void ListenForClients()
		{
			while (_isRunning)
			{
				try
				{
					TcpClient client = _listener.AcceptTcpClient();
					Thread clientThread = new Thread(HandleClientComm);
					clientThread.IsBackground = true;
					clientThread.Start(client);
				}
				catch { }
			}
		}

		private void HandleClientComm(object clientObj)
		{
			TcpClient client = (TcpClient)clientObj;
			try
			{
				NetworkStream stream = client.GetStream();
				BinaryReader reader = new BinaryReader(stream);
				BinaryWriter writer = new BinaryWriter(stream);

				// Читаем заголовок команды (Int32 длина + Строка UTF-8)
				int cmdLength = reader.ReadInt32();
				byte[] cmdBytes = reader.ReadBytes(cmdLength);
				string commandLine = Encoding.UTF8.GetString(cmdBytes);
				string[] cmdParts = commandLine.Split('|');
				string command = cmdParts[0];

				lock (_dbLock)
				{
					if (command == "ADD_ASSET")
					{
						// Прием бинарного пакета квантов от AssetEditor.aspx
						int textLength = reader.ReadInt32();
						byte[] textBytes = reader.ReadBytes(textLength);
						string[] parsedText = Encoding.UTF8.GetString(textBytes).Split('|');

						string qrString = parsedText[0];
						string invNumber = parsedText[1];
						string assetName = parsedText[2];
						string locationText = parsedText[3];
						string usersText = parsedText[4];
						string molIdStr = parsedText[5]; // Переданный Set_id выбранного МОЛ

						// Проверяем: существует ли уже ОМИ с таким QR-кодом?
						int targetSetId = 0;
						int qrValueIdForSearch = -1;
						foreach (var v in ValuesTable)
						{
							if (v.Value == qrString) { qrValueIdForSearch = v.Value_id; break; }
						}

						if (qrValueIdForSearch != -1)
						{
							var existingSet = SetsTable.Find(x => x.Name_id == Names.QRCode && x.Value_id == qrValueIdForSearch);
							if (existingSet != null) targetSetId = existingSet.Set_id;
						}

						// Если объект новый — генерируем ему уникальный Set_id, иначе используем старый
						bool isNew = (targetSetId == 0);
						if (isNew) targetSetId = GetNextUniqueSetId();

						// 1. Привязываем QR-код к Set_id (только для новых объектов)
						if (isNew)
						{
							int qrValId = GetOrCreateValueId(qrString, false);
							SetsTable.Add(new SetsRecord { Set_id = targetSetId, Name_id = Names.QRCode, Value_id = qrValId });
						}

						// 2. Обновляем текстовые атомарные свойства через перезапись Value_id
						UpdateOrAddProperty(targetSetId, Names.InventoryNumber, invNumber, false);
						UpdateOrAddProperty(targetSetId, Names.AssetsName, assetName, false);
						UpdateOrAddProperty(targetSetId, Names.AssetUsers, usersText, false);

						// 3. Привязываем МОЛ (волшебное поле — пишем Set_id пачки МОЛ напрямую в Value_id)
						int molIdInt = int.Parse(molIdStr);
						UpdateOrAddDirectLinkProperty(targetSetId, Names.ResponsibleAssets, molIdInt);

						// 4. ТРИЗ-выделение Местоположения в отдельную сущность для ведения истории
						if (!string.IsNullOrEmpty(locationText))
						{
							int locationSetId = GetNextUniqueSetId(); // Выделенный Set_id для этой точки истории
							int locTextValId = GetOrCreateValueId(locationText, true);
							int locTimeValId = GetOrCreateValueId(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), true);

							// Пишем пачку свойств локации
							SetsTable.Add(new SetsRecord { Set_id = locationSetId, Name_id = Names.AssetLocationText, Value_id = locTextValId });
							SetsTable.Add(new SetsRecord { Set_id = locationSetId, Name_id = Names.LocationDateTime, Value_id = locTimeValId });

							// Связываем ОМИ с этой новой сущностью перемещения (пишем locationSetId в Value_id)
							SetsTable.Add(new SetsRecord { Set_id = targetSetId, Name_id = Names.AssetLocationID, Value_id = locationSetId });
						}

						// 5. Принимаем серию бинарных сжатых фотографий
						int photoCount = reader.ReadInt32();
						for (int i = 0; i < photoCount; i++)
						{
							int photoLength = reader.ReadInt32();
							byte[] photoBytes = reader.ReadBytes(photoLength);

							string uniquePhotoName = Guid.NewGuid().ToString() + ".jpg";
							File.WriteAllBytes(Path.Combine(_photosPath, uniquePhotoName), photoBytes);

							int photoValueId = GetOrCreateValueId(uniquePhotoName, true);
							SetsTable.Add(new SetsRecord { Set_id = targetSetId, Name_id = Names.Photo, Value_id = photoValueId });
						}

						SaveDatabaseToDisk();
						SendStringResponse(writer, "SUCCESS");
					}
					else if (command == "CHECK_EXISTS")
					{
						string qrString = cmdParts[1];
						int qrValId = -1;
						foreach (var v in ValuesTable) { if (v.Value == qrString) { qrValId = v.Value_id; break; } }

						var qrRecord = (qrValId != -1) ? SetsTable.Find(x => x.Name_id == Names.QRCode && x.Value_id == qrValId) : null;

						if (qrRecord != null)
						{
							int id = qrRecord.Set_id;
							string name = GetPropertyValue(id, Names.AssetsName);
							string inv = GetPropertyValue(id, Names.InventoryNumber);
							string usrs = GetPropertyValue(id, Names.AssetUsers);
							string molId = GetDirectLinkValue(id, Names.ResponsibleAssets).ToString();
							// ТРИЗ-ХРОНОЛОГИЯ: Извлекаем из графа самое последнее актуальное местоположение
							string lastLoc = GetLatestLocationText(id);
							// Собираем старые фотографии в Base64 для передачи в миниатюры телефона
							List<string> base64Photos = new List<string>();
							var pRecs = SetsTable.FindAll(x => x.Set_id == id && x.Name_id == Names.Photo);
							foreach (var p in pRecs)
							{
								var vRec = ValuesTable.Find(v => v.Value_id == p.Value_id);
								if (vRec != null && File.Exists(Path.Combine(_photosPath, vRec.Value)))
								{
									byte[] b = File.ReadAllBytes(Path.Combine(_photosPath, vRec.Value));
									base64Photos.Add("data:image/jpeg;base64," + Convert.ToBase64String(b));
								}
							}
							string photosPayload = string.Join("|||", base64Photos.ToArray());
							SendStringResponse(writer, string.Format("FOUND|{0}|{1}|{2}|{3}|{4}|{5}", name, inv, usrs, molId, lastLoc, photosPayload));
						}
						else { SendStringResponse(writer, "NOT_FOUND"); }
					}
					else if (command == "MOVE_ASSET")
					{
						// Сценарий перемещения ОМИ с формы MoveAssets.aspx
						string qrString = cmdParts[1];
						string newLocation = cmdParts[2];
						int qrValId = -1;
						foreach (var v in ValuesTable) { if (v.Value == qrString) { qrValId = v.Value_id; break; } }
						var qrRecord = SetsTable.Find(x => x.Name_id == Names.QRCode && x.Value_id == qrValId);
						if (qrRecord != null)
						{
							int targetSetId = qrRecord.Set_id;
							// Создаем отдельную сущность местоположения
							int locationSetId = GetNextUniqueSetId();
							int locTextValId = GetOrCreateValueId(newLocation, true);
							int locTimeValId = GetOrCreateValueId(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), true);
							SetsTable.Add(new SetsRecord { Set_id = locationSetId, Name_id = Names.AssetLocationText, Value_id = locTextValId });
							SetsTable.Add(new SetsRecord { Set_id = locationSetId, Name_id = Names.LocationDateTime, Value_id = locTimeValId });
							// Дописываем ссылку на эту локацию в пачку ОМИ (накопление истории)
							SetsTable.Add(new SetsRecord { Set_id = targetSetId, Name_id = Names.AssetLocationID, Value_id = locationSetId });
							SaveDatabaseToDisk();
							SendStringResponse(writer, "SUCCESS");
						}
						else { SendStringResponse(writer, "ERROR_NOT_FOUND"); }
					}
					else if (command == "GET_MOL_LIST")
					{
						// Извлекаем все пачки МОЛ по маркеру должности (ResponsibleAssetsPosition)
						var molPositions = SetsTable.FindAll(x => x.Name_id == Names.ResponsibleAssetsPosition);
						var uniqueMolIds = new List<int>();
						foreach (var m in molPositions) { if (!uniqueMolIds.Contains(m.Set_id)) uniqueMolIds.Add(m.Set_id); }
						writer.Write(uniqueMolIds.Count);
						foreach (int mid in uniqueMolIds)
						{
							string ln = GetPropertyValue(mid, Names.LastName);
							string fn = GetPropertyValue(mid, Names.FirstName);
							string mn = GetPropertyValue(mid, Names.MiddleName);
							string pos = GetPropertyValue(mid, Names.ResponsibleAssetsPosition);
							string fullFio = string.Format("{0} {1}.{2}. ({3})", ln, string.IsNullOrEmpty(fn) ? "" : fn.Substring(0, 1), string.IsNullOrEmpty(mn) ? "" : mn.Substring(0, 1), pos);
							byte[] fioBytes = Encoding.UTF8.GetBytes(fullFio);
							writer.Write(mid);
							writer.Write(fioBytes.Length);
							writer.Write(fioBytes);
						}
						writer.Flush();
					}
					else if (command == "ADD_MOL" || command == "UPDATE_MOL")
					{
						int molSetId = (command == "ADD_MOL") ? GetNextUniqueSetId() : int.Parse(cmdParts[1]);
						int idxShift = (command == "ADD_MOL") ? 0 : 1;
						UpdateOrAddProperty(molSetId, Names.LastName, cmdParts[1 + idxShift], false);
						UpdateOrAddProperty(molSetId, Names.FirstName, cmdParts[2 + idxShift], false);
						UpdateOrAddProperty(molSetId, Names.MiddleName, cmdParts[3 + idxShift], false);
						UpdateOrAddProperty(molSetId, Names.ResponsibleAssetsPosition, cmdParts[4 + idxShift], false);
						SaveDatabaseToDisk();
						SendStringResponse(writer, "SUCCESS");
					}
					else if (command == "DELETE_MOL")
					{
						int molId = int.Parse(cmdParts[1]);
						// Защитная проверка: привязан ли этот МОЛ к какому-то имуществу?
						bool isLinked = SetsTable.Exists(x => x.Name_id == Names.ResponsibleAssets && x.Value_id == molId);
						if (isLinked)
						{
							SendStringResponse(writer, "DEPENDENCY_ERROR");
						}
						else
						{
							SetsTable.RemoveAll(x => x.Set_id == molId);
							SaveDatabaseToDisk();
							SendStringResponse(writer, "SUCCESS");
						}
					}
					else if (command == "GET_ALL_VIEWER_DATA")
					{
						// Трансляция всей базы в настольное приложение TrizDBViewer
						MemoryStream ms = new MemoryStream();
						BinaryWriter bw = new BinaryWriter(ms);
						bw.Write(SetsTable.Count);
						foreach (var s in SetsTable) { bw.Write(s.Set_id); bw.Write((int)s.Name_id); bw.Write(s.Value_id); }
						bw.Write(ValuesTable.Count);
						foreach (var v in ValuesTable) { bw.Write(v.Value_id); bw.Write(v.Value); }
						byte[] payload = ms.ToArray();
						writer.Write(payload.Length);
						writer.Write(payload);
						writer.Flush();
					}
					else if (command == "GET_PHOTOS")
					{
						int targetId = int.Parse(cmdParts[1]);
						var photos = SetsTable.FindAll(x => x.Set_id == targetId && x.Name_id == Names.Photo);
						writer.Write(photos.Count);
						foreach (var p in photos)
						{
							var vRec = ValuesTable.Find(v => v.Value_id == p.Value_id);
							byte[] bytes = new byte[0];
							if (vRec != null && File.Exists(Path.Combine(_photosPath, vRec.Value)))
							{
								bytes = File.ReadAllBytes(Path.Combine(_photosPath, vRec.Value));
							}
							writer.Write(bytes.Length);
							if (bytes.Length > 0) writer.Write(bytes);
						}
						writer.Flush();
					}
					else if (command == "UPDATE_VALUE")
					{
						int setId = int.Parse(cmdParts[1]);
						int nameIdInt = int.Parse(cmdParts[2]);
						string newValue = cmdParts[3];
						UpdateOrAddProperty(setId, (Names)nameIdInt, newValue, false);
						SaveDatabaseToDisk();
						SendStringResponse(writer, "SUCCESS");
					}
					else if (command == "UPDATE_LOCATION_TEXT")
					{
						int valueId = int.Parse(cmdParts[1]);
						string newText = cmdParts[2];
						var vRec = ValuesTable.Find(v => v.Value_id == valueId);
						if (vRec != null) vRec.Value = newText;
						SaveDatabaseToDisk();
						SendStringResponse(writer, "SUCCESS");
					}
					else if (command == "DELETE_ASSET")
					{
						int targetId = int.Parse(cmdParts[1]);
						// 1. Каскадно зачищаем файлы картинок этого ОМИ с диска сервера
						var photos = SetsTable.FindAll(x => x.Set_id == targetId && x.Name_id == Names.Photo);
						foreach (var p in photos)
						{
							var vRec = ValuesTable.Find(v => v.Value_id == p.Value_id);
							if (vRec != null)
							{
								string path = Path.Combine(_photosPath, vRec.Value);
								if (File.Exists(path)) File.Delete(path);
							}
						}
						// 2. Находим и зачищаем все вложенные сущности местоположений этого объекта
						var locations = SetsTable.FindAll(x => x.Set_id == targetId && x.Name_id == Names.AssetLocationID);
						foreach (var l in locations)
						{
							SetsTable.RemoveAll(x => x.Set_id == l.Value_id); // Удаляем пачки локаций
						}
						// 3. Удаляем все свойства самого объекта ОМИ
						SetsTable.RemoveAll(x => x.Set_id == targetId);
						SaveDatabaseToDisk();
						SendStringResponse(writer, "SUCCESS");
					}
				}
			}
			catch { }
			finally { client.Close(); }
		}
		// Вспомогательные методы извлечения и записи свойств
		private string GetPropertyValue(int setId, Names name)
		{
			var rec = SetsTable.Find(x => x.Set_id == setId && x.Name_id == name);
			if (rec != null)
			{
				var vRec = ValuesTable.Find(v => v.Value_id == rec.Value_id);
				if (vRec != null) return vRec.Value;
			}
			return "";
		}
		private int GetDirectLinkValue(int setId, Names name)
		{
			var rec = SetsTable.Find(x => x.Set_id == setId && x.Name_id == name);
			return (rec != null) ? rec.Value_id : 0;
		}
		private string GetLatestLocationText(int assetSetId)
		{
			var locLinks = SetsTable.FindAll(x => x.Set_id == assetSetId && x.Name_id == Names.AssetLocationID);
			if (locLinks.Count == 0) return "Местоположение не зарегистрировано";
			// Так как список заполняется хронологически, последняя ссылка в списке — самая актуальная
			int latestLocationSubSetId = locLinks[locLinks.Count - 1].Value_id;
			var txtRec = SetsTable.Find(x => x.Set_id == latestLocationSubSetId && x.Name_id == Names.AssetLocationText);
			if (txtRec != null)
			{
				var vRec = ValuesTable.Find(v => v.Value_id == txtRec.Value_id);
				if (vRec != null) return vRec.Value;
			}
			return "Не указано";
		}
		private void UpdateOrAddProperty(int setId, Names name, string valueText, bool forceUnique)
		{
			int valId = GetOrCreateValueId(valueText, forceUnique);
			var record = SetsTable.Find(x => x.Set_id == setId && x.Name_id == name);
			if (record != null) record.Value_id = valId;
			else SetsTable.Add(new SetsRecord { Set_id = setId, Name_id = name, Value_id = valId });
		}
		private void UpdateOrAddDirectLinkProperty(int setId, Names name, int targetSubSetId)
		{
			var record = SetsTable.Find(x => x.Set_id == setId && x.Name_id == name);
			if (record != null) record.Value_id = targetSubSetId;
			else SetsTable.Add(new SetsRecord { Set_id = setId, Name_id = name, Value_id = targetSubSetId });
		}
		private int GetOrCreateValueId(string valueText, bool forceUnique)
		{
			if (!forceUnique)
			{
				foreach (var record in ValuesTable)
				{
					if (record.Value == valueText) return record.Value_id;
				}
			}
			int newId = ValuesTable.Count + 1;
			ValuesTable.Add(new ValuesRecord { Value_id = newId, Value = valueText });
			return newId;
		}
		private void SendStringResponse(BinaryWriter writer, string text)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			writer.Write(bytes.Length);
			writer.Write(bytes);
			writer.Flush();
		}
		private void SaveDatabaseToDisk()
		{
			using (BinaryWriter bw = new BinaryWriter(File.Open(Path.Combine(_basePath, "Sets.table"), FileMode.Create)))
			{
				bw.Write(SetsTable.Count);
				foreach (var s in SetsTable) { bw.Write(s.Set_id); bw.Write((int)s.Name_id); bw.Write(s.Value_id); }
			}
			using (BinaryWriter bw = new BinaryWriter(File.Open(Path.Combine(_basePath, "Values.table"), FileMode.Create)))
			{
				bw.Write(ValuesTable.Count);
				foreach (var v in ValuesTable) { bw.Write(v.Value_id); bw.Write(v.Value); }
			}
		}
		private void LoadDatabaseFromDisk()
		{
			string sFile = Path.Combine(_basePath, "Sets.table");
			string vFile = Path.Combine(_basePath, "Values.table");
			if (File.Exists(sFile))
			{
				using (BinaryReader br = new BinaryReader(File.Open(sFile, FileMode.Open)))
				{
					int c = br.ReadInt32(); SetsTable.Clear();
					for (int i = 0; i < c; i++) SetsTable.Add(new SetsRecord { Set_id = br.ReadInt32(), Name_id = (Names)br.ReadInt32(), Value_id = br.ReadInt32() });
				}
			}
			if (File.Exists(vFile))
			{
				using (BinaryReader br = new BinaryReader(File.Open(vFile, FileMode.Open)))
				{
					int c = br.ReadInt32(); ValuesTable.Clear();
					for (int i = 0; i < c; i++) ValuesTable.Add(new ValuesRecord { Value_id = br.ReadInt32(), Value = br.ReadString() });
				}
			}
		}
		protected override void OnStop()
		{
			_isRunning = false;
			if (_listener != null) _listener.Stop();
			if (_listenerThread != null && _listenerThread.IsAlive) _listenerThread.Join(2000);
			lock (_dbLock) { SaveDatabaseToDisk(); }
		}
	}
}