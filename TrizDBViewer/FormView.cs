namespace TrizDBViewer
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Net.Sockets;
	using System.Text;
	using System.Threading;
	using System.Windows.Forms;

	public partial class FormView : Form
	{
		private List<AssetRow> _allAssets = new List<AssetRow>();
		private Dictionary<int, string> _allMolsDictionary = new Dictionary<int, string>();
		private BindingSource _bindingSource = new BindingSource();
		private string _serverIp = "127.0.0.1";
		private int _serverPort = 65525;
		private int _lastSelectedSetId = -1; // Защита от потоковой гонки картинок


		public FormView()
		{
			InitializeComponent();
			_bindingSource.DataSource = _allAssets;
			dgv.DataSource = _bindingSource;
		}

		private void btnRefresh_Click(object sender, EventArgs e)
		{
			RefreshDataFromDatabase();
		}

		private void RefreshDataFromDatabase()
		{
			try
			{
				using (TcpClient socket = new TcpClient(_serverIp, _serverPort))
				{
					var stream = socket.GetStream(); var w = new BinaryWriter(stream); var r = new BinaryReader(stream);

					byte[] cmd = Encoding.UTF8.GetBytes("GET_ALL_VIEWER_DATA");
					w.Write(cmd.Length); w.Write(cmd); w.Flush();

					int len = r.ReadInt32(); byte[] rawDb = r.ReadBytes(len);

					MemoryStream ms = new MemoryStream(rawDb);
					BinaryReader br = new BinaryReader(ms);

					// Считываем SetsTable
					int setsCount = br.ReadInt32();
					var sets = new List<Tuple<int, int, int>>();
					for (int i = 0; i < setsCount; i++)
					{
						sets.Add(new Tuple<int, int, int>(br.ReadInt32(), br.ReadInt32(), br.ReadInt32()));
					}

					// Считываем ValuesTable
					int valsCount = br.ReadInt32();
					var vals = new Dictionary<int, string>();
					for (int i = 0; i < valsCount; i++)
					{
						vals.Add(br.ReadInt32(), br.ReadString());
					}

					// Шаг 1. Предварительно собираем справочник всех МОЛ в памяти для подстановки в ОМИ
					_allMolsDictionary.Clear();
					var molPositions = sets.FindAll(x => x.Item2 == 11); // Маркер ResponsibleAssetsPosition
					foreach (var m in molPositions)
					{
						int mid = m.Item1;
						var lnRec = sets.Find(x => x.Item1 == mid && x.Item2 == 8);
						var fnRec = sets.Find(x => x.Item1 == mid && x.Item2 == 9);
						var mnRec = sets.Find(x => x.Item1 == mid && x.Item2 == 10);
						var posRec = sets.Find(x => x.Item1 == mid && x.Item2 == 11);

						string ln = (lnRec != null && vals.ContainsKey(lnRec.Item3)) ? vals[lnRec.Item3] : "";
						string fn = (fnRec != null && vals.ContainsKey(fnRec.Item3)) ? vals[fnRec.Item3] : "";
						string mn = (mnRec != null && vals.ContainsKey(mnRec.Item3)) ? vals[mnRec.Item3] : "";
						string pos = (posRec != null && vals.ContainsKey(posRec.Item3)) ? vals[posRec.Item3] : "";

						string initials = string.Format("{0} {1}.{2}. ({3})", ln, string.IsNullOrEmpty(fn) ? "" : fn.Substring(0, 1), string.IsNullOrEmpty(mn) ? "" : mn.Substring(0, 1), pos);
						if (!_allMolsDictionary.ContainsKey(mid)) _allMolsDictionary.Add(mid, initials);
					}

					// Шаг 2. Собираем уникальные Set_id Объектов Имущества (маркируются QRCode = 1)
					var assetRecords = sets.FindAll(x => x.Item2 == 1);
					var uniqueAssetIds = assetRecords.Select(x => x.Item1).Distinct().ToList();

					_allAssets.Clear();
					foreach (int id in uniqueAssetIds)
					{
						AssetRow row = new AssetRow { Set_id = id };
						var myProps = sets.FindAll(x => x.Item1 == id);

						var qr = myProps.Find(x => x.Item2 == 1); if (qr != null && vals.ContainsKey(qr.Item3)) row.QrCodeText = vals[qr.Item3];
						var inv = myProps.Find(x => x.Item2 == 2); if (inv != null && vals.ContainsKey(inv.Item3)) row.InventoryNumber = vals[inv.Item3];
						var name = myProps.Find(x => x.Item2 == 3); if (name != null && vals.ContainsKey(name.Item3)) row.AssetsName = vals[name.Item3];
						var usr = myProps.Find(x => x.Item2 == 7); if (usr != null && vals.ContainsKey(usr.Item3)) row.Users = vals[usr.Item3];

						// Волшебное поле связи с МОЛ
						var molLink = myProps.Find(x => x.Item2 == 5);
						if (molLink != null)
						{
							row.ResponsiblePersonSetId = molLink.Item3; // Содержит Set_id пачки МОЛ
							if (_allMolsDictionary.ContainsKey(molLink.Item3)) row.ResponsiblePersonFio = _allMolsDictionary[molLink.Item3];
						}
						if (string.IsNullOrEmpty(row.ResponsiblePersonFio)) row.ResponsiblePersonFio = "[ Не назначено ]";

						// Собираем историю перемещений через волшебные поля связей локаций хронологически
						var locLinks = myProps.FindAll(x => x.Item2 == 6); // Ссылки на Set_id сущностей локаций
						foreach (var link in locLinks)
						{
							int locSubSetId = link.Item3;
							var txtRec = sets.Find(x => x.Item1 == locSubSetId && x.Item2 == 12);
							var timeRec = sets.Find(x => x.Item1 == locSubSetId && x.Item2 == 13);
							if (txtRec != null && vals.ContainsKey(txtRec.Item3))
							{
								string locText = vals[txtRec.Item3];
								string timeText = (timeRec != null && vals.ContainsKey(timeRec.Item3)) ? vals[timeRec.Item3] : "Время неизвестно";
								row.LocationHistory.Add(new LocationHistoryItem { ValueRecordId = txtRec.Item3, LocationText = locText, TimestampText = timeText });
							}
						}
						_allAssets.Add(row);
					}
				}
				_bindingSource.DataSource = null; _bindingSource.DataSource = _allAssets;
				ApplyGridFiltering("");
			}
			catch (Exception ex) { MessageBox.Show("Не удалось подключиться к службе TrizDB: " + ex.Message, "Сбой сети", MessageBoxButtons.OK, MessageBoxIcon.Error); }
		}

		private void ApplyGridFiltering(string query)
		{
			query = query.Trim().ToLower();
			if (string.IsNullOrEmpty(query)) { _bindingSource.DataSource = _allAssets; return; }
			// Поиск без учета регистра по Фрагментам (Наименование, Инвентарный №, Пользователи) C# 4.0 совместимо
			var filtered = _allAssets.Where(x =>
			(x.AssetsName != null && x.AssetsName.ToLower().Contains(query)) ||
			(x.InventoryNumber != null && x.InventoryNumber.ToLower().Contains(query)) ||
			(x.Users != null && x.Users.ToLower().Contains(query))
			).ToList();
			_bindingSource.DataSource = filtered;
		}
		private void dgv_SelectionChanged(object sender, EventArgs e)
		{
			DataGridView dgv = (DataGridView)this.Controls.Find("dgv", true).FirstOrDefault();
			FlowLayoutPanel flow = (FlowLayoutPanel)this.Controls.Find("flowPhotos", true).FirstOrDefault();
			ListBox lst = (ListBox)this.Controls.Find("lstHistory", true).FirstOrDefault();
			if (dgv == null || dgv.CurrentRow == null || flow == null || lst == null) return;
			AssetRow row = (AssetRow)dgv.CurrentRow.DataBoundItem;
			if (row == null) return;
			// Защитный ТРИЗ-барьер от наложения параллельных гонок при загрузке формы
			if (_lastSelectedSetId == row.Set_id) return;
			_lastSelectedSetId = row.Set_id;
			flow.Controls.Clear(); lst.Items.Clear();
			int maxLineWidth = 0;
			// Выводим текстовую историю перемещений
			foreach (var h in row.LocationHistory)
			{
				string itemText = string.Format("[{0}] Локация: {1}", h.TimestampText, h.LocationText);
				lst.Items.Add(itemText);
				// Динамический расчет горизонтальной прокрутки ListBox под длину строки
				Size s = TextRenderer.MeasureText(itemText, lst.Font);
				if (s.Width > maxLineWidth) maxLineWidth = s.Width;
			}
			lst.HorizontalExtent = maxLineWidth + 12;
			int currentId = row.Set_id;
			new Thread(() =>
			{
				try
				{
					using (TcpClient s = new TcpClient(_serverIp, _serverPort))
					{
						var stream = s.GetStream(); var w = new BinaryWriter(stream); var r = new BinaryReader(stream);
						byte[] cmd = Encoding.UTF8.GetBytes("GET_PHOTOS|" + currentId);
						w.Write(cmd.Length); w.Write(cmd); w.Flush();
						int count = r.ReadInt32();
						for (int i = 0; i < count; i++)
						{
							int len = r.ReadInt32(); byte[] imgBytes = r.ReadBytes(len);
							this.Invoke(new Action(() =>
							{
								// Если пока качался поток, пользователь уже переключил строку таблицы — игнорируем
								if (_lastSelectedSetId == currentId)
								{
									PictureBox pic = new PictureBox { Width = 110, Height = 110, SizeMode = PictureBoxSizeMode.Zoom, Image = Image.FromStream(new MemoryStream(imgBytes)), Margin = new Padding(3), Cursor = Cursors.Hand };
									pic.Click += (ss, ee) => OpenFullScreenViewer(pic.Image);
									flow.Controls.Add(pic);
								}
							}));
						}
					}
				}
				catch { }
			}).Start();
		}
		private void OpenFullScreenViewer(Image fullImage)
		{
			Form frm = new Form { WindowState = FormWindowState.Maximized, FormBorderStyle = FormBorderStyle.None, BackColor = Color.Black };
			PictureBox p = new PictureBox { Image = fullImage, Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
			p.Click += (s, e) => frm.Close();
			frm.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) frm.Close(); };
			frm.Controls.Add(p); frm.ShowDialog();
		}
		private void dgv_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView dgv = (DataGridView)sender;
			AssetRow row = (AssetRow)dgv.Rows[e.RowIndex].DataBoundItem;
			// Совместимое с компилятором Visual Studio 2010 (C# 4.0) извлечение значения ячейки
			object valObj = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
			string newVal = (valObj != null) ? valObj.ToString() : "";
			int nameId = (e.ColumnIndex == 1) ? 2 : (e.ColumnIndex == 2) ? 3 : 7; // Сопоставление с enum Names СУБД
			ExecuteServerUpdate(string.Format("UPDATE_VALUE|{0}|{1}|{2}", row.Set_id, nameId, newVal));
		}
		private void btnEditLoc_Click(object sender, EventArgs e)
		{
			ListBox lst = (ListBox)this.Controls.Find("lstHistory", true).FirstOrDefault();
			DataGridView dgv = (DataGridView)this.Controls.Find("dgv", true).FirstOrDefault();
			if (lst == null || lst.SelectedIndex < 0 || dgv == null || dgv.CurrentRow == null) return;
			AssetRow row = (AssetRow)dgv.CurrentRow.DataBoundItem;
			var historyItem = row.LocationHistory[lst.SelectedIndex];
			Form f = new Form { Width = 400, Height = 180, Text = "Правка опечатки местоположения", StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };
			TextBox t = new TextBox { Left = 20, Top = 20, Width = 340, Text = historyItem.LocationText, Multiline = true, Height = 60 };
			Button b = new Button { Text = "Сохранить изменения", Left = 20, Top = 95, Width = 160, Height = 30, DialogResult = DialogResult.OK };
			f.Controls.AddRange(new Control[] { t, b });
			if (f.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(t.Text.Trim()))
			{
				string resp = ExecuteServerUpdate(string.Format("UPDATE_LOCATION_TEXT|{0}|{1}", historyItem.ValueRecordId, t.Text.Trim()));
				if (resp == "SUCCESS") RefreshDataFromDatabase();
			}
		}
		private void btnDelete_Click(object sender, EventArgs e)
		{
			DataGridView dgv = (DataGridView)this.Controls.Find("dgv", true).FirstOrDefault();
			if (dgv == null || dgv.CurrentRow == null) return;
			AssetRow row = (AssetRow)dgv.CurrentRow.DataBoundItem;
			if (MessageBox.Show("Вы действительно хотите БЕЗВОЗВРАТНО удалить этот имущественный объект, каскадно стереть все файлы его фотографий с сервера и стереть всю историю перемещений?", "Каскадная зачистка СУБД TrizDB", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
			{
				string r = ExecuteServerUpdate("DELETE_ASSET|" + row.Set_id);
				if (r == "SUCCESS") { _lastSelectedSetId = -1; RefreshDataFromDatabase(); }
			}
		}
		private void btnMols_Click(object sender, EventArgs e)
		{
			ResponsiblePersonsForm f = new ResponsiblePersonsForm(_serverIp, _serverPort);
			f.ShowDialog();
			RefreshDataFromDatabase(); // Перечитываем базу для обновления ФИО МОЛ в сетке
		}
		private string ExecuteServerUpdate(string cmd)
		{
			try
			{
				using (TcpClient s = new TcpClient(_serverIp, _serverPort))
				{
					var stream = s.GetStream(); var w = new BinaryWriter(stream); var r = new BinaryReader(stream);
					byte[] cBytes = Encoding.UTF8.GetBytes(cmd);
					w.Write(cBytes.Length); w.Write(cBytes); w.Flush();
					int len = r.ReadInt32(); string resp = Encoding.UTF8.GetString(r.ReadBytes(len));
					return resp;
				}
			}
			catch (Exception ex) { MessageBox.Show("Связь со службой СУБД разорвана: " + ex.Message); return "ERROR"; }
		}

		private void txtSearch_TextChanged(object sender, EventArgs e)
		{
			ApplyGridFiltering(txtSearch.Text);
		}
	}
}
