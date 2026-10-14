using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Linq;

namespace TrizDBViewer
{
	public class ResponsiblePersonsForm : Form
	{
		private string _ip; int _port;
		private DataGridView _dgvMols;
		private TextBox _txtLn, _txtFn, _txtMn, _txtPos;
		private Button _btnAdd, _btnEdit, _btnDel;
		private List<MolItem> _mols = new List<MolItem>();

		public class MolItem
		{
			public int Set_id { get; set; }
			public string LastName { get; set; }
			public string FirstName { get; set; }
			public string MiddleName { get; set; }
			public string Position { get; set; }
		}

		public ResponsiblePersonsForm(string ip, int port)
		{
			_ip = ip; _port = port;
			InitializeComponentLayout();
			LoadMolsFromNetwork();
		}

		private void InitializeComponentLayout()
		{
			this.Text = "Справочник материально ответственных лиц (МОЛ)";
			this.Size = new Size(680, 480);
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;

			_dgvMols = new DataGridView { Left = 15, Top = 15, Width = 635, Height = 220, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, MultiSelect = false, AutoGenerateColumns = false };
			_dgvMols.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { DataPropertyName = "Set_id", HeaderText = "ID пачки", ReadOnly = true, Width = 60 },
                new DataGridViewTextBoxColumn { DataPropertyName = "LastName", HeaderText = "Фамилия", Width = 130 },
                new DataGridViewTextBoxColumn { DataPropertyName = "FirstName", HeaderText = "Имя", Width = 110 },
                new DataGridViewTextBoxColumn { DataPropertyName = "MiddleName", HeaderText = "Отчество", Width = 110 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Position", HeaderText = "Должность МОЛ", Width = 180 }
            });
			_dgvMols.SelectionChanged += (s, e) =>
			{
				if (_dgvMols.CurrentRow == null) return;
				var m = (MolItem)_dgvMols.CurrentRow.DataBoundItem;
				_txtLn.Text = m.LastName; _txtFn.Text = m.FirstName; _txtMn.Text = m.MiddleName; _txtPos.Text = m.Position;
			};
			this.Controls.Add(_dgvMols);

			// Поля ввода
			Label l1 = new Label { Text = "Фамилия:", Left = 15, Top = 255, Width = 70 };
			_txtLn = new TextBox { Left = 90, Top = 252, Width = 180 };
			Label l2 = new Label { Text = "Имя:", Left = 15, Top = 290, Width = 70 };
			_txtFn = new TextBox { Left = 90, Top = 287, Width = 180 };
			Label l3 = new Label { Text = "Отчество:", Left = 15, Top = 325, Width = 70 };
			_txtMn = new TextBox { Left = 90, Top = 322, Width = 180 };
			Label l4 = new Label { Text = "Должность:", Left = 300, Top = 255, Width = 80 };
			_txtPos = new TextBox { Left = 390, Top = 252, Width = 260 };

			this.Controls.AddRange(new Control[] { l1, _txtLn, l2, _txtFn, l3, _txtMn, l4, _txtPos });

			// Кнопки действий
			_btnAdd = new Button { Text = "➕ Добавить МОЛ", Left = 15, Top = 380, Width = 130, Height = 35 };
			_btnAdd.Click += (s, e) => ExecuteMolAction("ADD_MOL");

			_btnEdit = new Button { Text = "📝 Изменить выбранного", Left = 155, Top = 380, Width = 170, Height = 35 };
			_btnEdit.Click += (s, e) =>
			{
				if (_dgvMols.CurrentRow == null) return;
				ExecuteMolAction("UPDATE_MOL");
			};

			_btnDel = new Button { Text = "❌ Удалить МОЛ", Left = 520, Top = 380, Width = 130, Height = 35, ForeColor = Color.Red };
			_btnDel.Click += btnDel_Click;

			this.Controls.AddRange(new Control[] { _btnAdd, _btnEdit, _btnDel });
		}

		private void LoadMolsFromNetwork()
		{
			try
			{
				using (TcpClient client = new TcpClient(_ip, _port))
				{
					var stream = client.GetStream(); var w = new BinaryWriter(stream); var r = new BinaryReader(stream);
					byte[] cmd = Encoding.UTF8.GetBytes("GET_ALL_VIEWER_DATA");
					w.Write(cmd.Length); w.Write(cmd); w.Flush();

					int len = r.ReadInt32(); byte[] raw = r.ReadBytes(len);
					MemoryStream ms = new MemoryStream(raw); BinaryReader br = new BinaryReader(ms);

					int setsCount = br.ReadInt32();
					var sets = new List<Tuple<int, int, int>>();
					for (int i = 0; i < setsCount; i++) sets.Add(new Tuple<int, int, int>(br.ReadInt32(), br.ReadInt32(), br.ReadInt32()));

					int valsCount = br.ReadInt32();
					var vals = new Dictionary<int, string>();
					for (int i = 0; i < valsCount; i++) vals.Add(br.ReadInt32(), br.ReadString());

					// Извлекаем пачки МОЛ по маркеру должности (11 = ResponsibleAssetsPosition)
					var uniqueMolIds = sets.FindAll(x => x.Item2 == 11).Select(x => x.Item1).Distinct().ToList();
					_mols.Clear();

					foreach (int id in uniqueMolIds)
					{
						var m = new MolItem { Set_id = id };
						var ln = sets.Find(x => x.Item1 == id && x.Item2 == 8); if (ln != null && vals.ContainsKey(ln.Item3)) m.LastName = vals[ln.Item3];
						var fn = sets.Find(x => x.Item1 == id && x.Item2 == 9); if (fn != null && vals.ContainsKey(fn.Item3)) m.FirstName = vals[fn.Item3];
						var mn = sets.Find(x => x.Item1 == id && x.Item2 == 10); if (mn != null && vals.ContainsKey(mn.Item3)) m.MiddleName = vals[mn.Item3];
						var pos = sets.Find(x => x.Item1 == id && x.Item2 == 11); if (pos != null && vals.ContainsKey(pos.Item3)) m.Position = vals[pos.Item3];
						_mols.Add(m);
					}
					_dgvMols.DataSource = null; _dgvMols.DataSource = _mols;
				}
			}
			catch { }
		}

		private void ExecuteMolAction(string action)
		{
			if (string.IsNullOrEmpty(_txtLn.Text.Trim()) || string.IsNullOrEmpty(_txtPos.Text.Trim()))
			{
				MessageBox.Show("Поля Фамилия и Должность обязательны!", "Отказ"); return;
			}

			string payload = "";
			if (action == "ADD_MOL")
			{
				payload = string.Format("ADD_MOL|{0}|{1}|{2}|{3}", _txtLn.Text.Trim(), _txtFn.Text.Trim(), _txtMiddleNameValue(), _txtPos.Text.Trim());
			}
			else
			{
				var m = (MolItem)_dgvMols.CurrentRow.DataBoundItem;
				payload = string.Format("UPDATE_MOL|{0}|{1}|{2}|{3}|{4}", m.Set_id, _txtLn.Text.Trim(), _txtFn.Text.Trim(), _txtMiddleNameValue(), _txtPos.Text.Trim());
			}

			try
			{
				using (TcpClient s = new TcpClient(_ip, _port))
				{
					var stream = s.GetStream(); var w = new BinaryWriter(stream); var r = new BinaryReader(stream);
					byte[] bytes = Encoding.UTF8.GetBytes(payload);
					w.Write(bytes.Length); w.Write(bytes); w.Flush();
					int len = r.ReadInt32(); string resp = Encoding.UTF8.GetString(r.ReadBytes(len));
					if (resp == "SUCCESS")
					{
						_txtLn.Text = ""; _txtFn.Text = ""; _txtMn.Text = ""; _txtPos.Text = "";
						LoadMolsFromNetwork();
						MessageBox.Show("Справочник МОЛ успешно синхронизирован с СУБД!", "Успех");
					}
				}
			}
			catch (Exception ex) { MessageBox.Show("Ошибка сокета СУБД: " + ex.Message); }
		}

		private string _txtMiddleNameValue() { return string.IsNullOrEmpty(_txtMn.Text.Trim()) ? " " : _txtMn.Text.Trim(); }

		private void btnDel_Click(object sender, EventArgs e)
		{
			if (_dgvMols.CurrentRow == null) return;
			var m = (MolItem)_dgvMols.CurrentRow.DataBoundItem;

			if (MessageBox.Show("Удалить выбранного сотрудника из справочника МОЛ?", "Запрос", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				try
				{
					using (TcpClient s = new TcpClient(_ip, _port))
					{
						var stream = s.GetStream(); var w = new BinaryWriter(stream); var r = new BinaryReader(stream);
						byte[] bytes = Encoding.UTF8.GetBytes("DELETE_MOL|" + m.Set_id);
						w.Write(bytes.Length); w.Write(bytes); w.Flush();

						int len = r.ReadInt32(); string resp = Encoding.UTF8.GetString(r.ReadBytes(len));
						if (resp == "SUCCESS") { LoadMolsFromNetwork(); }
						else if (resp == "DEPENDENCY_ERROR")
						{
							MessageBox.Show("Ошибка удаления: за данным МОЛ закреплены материальные объекты в СУБД! Сначала переназначьте объекты на другое лицо.", "Блокировка каскада", MessageBoxButtons.OK, MessageBoxIcon.Stop);
						}
					}
				}
				catch { }
			}
		}
	}
}