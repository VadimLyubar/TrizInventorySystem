using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FirstASP
{
	public partial class AssetCard : System.Web.UI.Page
	{
		private static string KeyFormData = string.Concat(typeof(AssetCard), "FormData");

		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				LoadResponsiblePersonsList();
				LoadFormData();
				LoadReturnedQR();
				VerifyAndConfigureFormState();
			}
		}

		protected void ButtonQR_Click(object sender, EventArgs e)
		{
			SaveFormData();
			this.CallWebForm(typeof(AssetCard), typeof(ScannerQR));
		}

		private void SaveFormData()
		{
			AssetCardFormData data = AssetCardFormData.CreateFromPage(this);
			Session[KeyFormData] = data;
		}

		private void LoadFormData()
		{
			if (Session[KeyFormData] != null)
			{
				((AssetCardFormData)Session[KeyFormData]).CopyFormDataToPage(this);
			}
		}

		private void LoadReturnedQR()
		{
			object result = this.ResultWebForm(typeof(AssetCard), typeof(ScannerQR));
			if (result != null) TextBoxQR.Text = result.ToString();
		}

		private void LoadResponsiblePersonsList()
		{
			ddlResponsiblePerson.Items.Clear();
			ddlResponsiblePerson.Items.Add(new ListItem("[ МОЛ не выбрано ]", "0"));
			try
			{
				SocketService socket = Application["NetworkSocket"] as SocketService;
				if (socket != null)
				{
					List<Tuple<int, string>> molList = socket.DownloadMOLList();
					foreach (var mol in molList)
					{
						ddlResponsiblePerson.Items.Add(new ListItem(mol.Item2, mol.Item1.ToString()));
					}
				}
			}
			catch { }
		}

		private void VerifyAndConfigureFormState()
		{
			if (string.IsNullOrEmpty(TextBoxQR.Text))
			{
				SetFormMode(false, "Ожидание ввода. Отсканируйте QR-код для начала.", false);
				return;
			}

			// Проверка формата строки числового QR-кода C# 4.0 совместимо
			int tempQrValue;
			bool isQrValid = (TextBoxQR.Text.Length == 6 && int.TryParse(TextBoxQR.Text, out tempQrValue));
			if (!isQrValid)
			{
				ShowInformer("Неподходящий QR-код! Ожидается строго 6 цифр.", false);
				SetFormMode(false, "", false);
				TextBoxQR.Style["border"] = "2px solid #f05252";
				return;
			}

			try
			{
				SocketService socket = Application["NetworkSocket"] as SocketService;
				if (socket == null) throw new Exception("Служба сокетов СУБД недоступна.");

				string response = socket.SendStringCommand("CHECK_EXISTS|" + TextBoxQR.Text);

				if (response.StartsWith("FOUND"))
				{
					// ================= РЕЖИМ 1: ОБЪЕКТ НАЙДЕН -> ТОЛЬКО ПРОСМОТР =================
					string[] parts = response.Split('|');
					string assetName = parts[1];
					string invNumber = parts[2];
					string currentUsers = parts[3];
					string currentMolId = parts[4];
					string lastLocation = parts[5];
					string base64PhotosPayload = parts[6];

					// Принудительно разворачиваем архивные данные в поля
					TextBoxInventoryNumber.Text = invNumber;
					TextBoxName.Text = assetName;
					TextBoxUsers.Text = currentUsers;
					TextBoxLocation.Text = lastLocation; // Самое свежее местоположение по времени сервера
					hfCachedPhotosBase64.Value = base64PhotosPayload; // Передаем картинки в JS-превью

					if (ddlResponsiblePerson.Items.FindByValue(currentMolId) != null)
						ddlResponsiblePerson.SelectedValue = currentMolId;

					// Сигнализируем JS-скрипту заблокировать крестики удаления фото
					hfIsReadOnlyMode.Value = "true";

					// Переключаем всю форму в режим "Только чтение" [ТРИЗ-Оптимизация]
					SetFormMode(false, "Архивная карточка СУБД TrizDB. Поля защищены от перезаписи.", true);
					pnlSystemInformer.Style["background-color"] = "#eff6ff";
					pnlSystemInformer.Style["border-left"] = "5px solid #3b82f6";
					pnlSystemInformer.Style["border-color"] = "#bfdbfe";
					lblInformerText.Style["color"] = "#1e40af";
					litInformerIcon.Text = "ℹ️";

					TextBoxQR.Style["border"] = "2px solid #3b82f6";
					ButtonSaveAsset.Text = "В главное меню";
					ButtonSaveAsset.Style["background-color"] = "#4b5563"; // Серая кнопка выхода

					// Очищаем сессию ввода, так как объект уже зафиксирован в базе
					Session[KeyFormData] = null;
				}
				else
				{
					// ================= РЕЖИМ 2: ОБЪЕКТА НЕТ -> ПЕРВИЧНОЕ ДОБАВЛЕНИЕ =================
					hfIsReadOnlyMode.Value = "false";
					SetFormMode(true, "Новый объект имущества. Заполните карточку первичного учета по ТРИЗ.", true);
					TextBoxQR.Style["border"] = "2px solid #31c48d";
					ButtonSaveAsset.Text = "Добавить объект в систему";
					ButtonSaveAsset.Style["background-color"] = "#16a34a"; // Зеленая кнопка сохранения
				}
			}
			catch (Exception ex)
			{
				ShowInformer("Ошибка соединения СУБД: " + ex.Message, false);
				SetFormMode(false, "", false);
			}
		}

		private void SetFormMode(bool fieldsEditable, string informerText, bool buttonEnabled)
		{
			TextBoxInventoryNumber.Enabled = fieldsEditable;
			TextBoxName.Enabled = fieldsEditable;
			TextBoxLocation.Enabled = fieldsEditable;
			TextBoxUsers.Enabled = fieldsEditable;
			ddlResponsiblePerson.Enabled = fieldsEditable;

			// Управляем видимостью двухкнопочного блока фото на клиенте
			divPhotoButtons.Style["display"] = fieldsEditable ? "flex" : "none";
			divPhotoLabel.Style["display"] = (fieldsEditable || !string.IsNullOrEmpty(hfCachedPhotosBase64.Value)) ? "block" : "none";

			ButtonSaveAsset.Enabled = buttonEnabled;

			if (!string.IsNullOrEmpty(informerText))
			{
				ShowInformer(informerText, true);
			}
		}

		private void ShowInformer(string text, bool isSuccess)
		{
			lblInformerText.Text = text; pnlSystemInformer.Visible = true;
			litInformerIcon.Text = isSuccess ? "✅" : "⚠️";
			pnlSystemInformer.Style["background-color"] = isSuccess ? "#f3faf7" : "#fdf2f2";
			pnlSystemInformer.Style["border-left"] = isSuccess ? "5px solid #31c48d" : "5px solid #f05252";
			pnlSystemInformer.Style["border-color"] = isSuccess ? "#def7ec" : "#f8b4b4";
			lblInformerText.Style["color"] = isSuccess ? "#03543f" : "#c81e1e";
		}

		protected void ButtonSaveAsset_Click(object sender, EventArgs e)
		{
			// Если кнопка была в режиме "В главное меню", просто возвращаем пользователя на старт без запросов в сокет
			if (ButtonSaveAsset.Text == "В главное меню")
			{
				Response.Redirect("Default.aspx");
				return;
			}

			AssetCardFormData outData = AssetCardFormData.CreateFromPage(this);
			if (string.IsNullOrEmpty(outData.QR)) return;

			try
			{
				SocketService socket = Application["NetworkSocket"] as SocketService;
				if (socket == null) throw new Exception("Сетевой модуль сокетов отключен.");

				// Отправляем пакет квантов первичного учета
				string response = socket.SendAssetPayload(
					outData.QR,
					outData.InventoryNumber.Trim(),
					outData.AssetsName.Trim(),
					outData.Location.Trim(),
					outData.Users.Trim(),
					ddlResponsiblePerson.SelectedValue,
					outData.RawPhotos
				);

				if (response == "SUCCESS")
				{
					ShowInformer("Объект имущества успешно зарегистрирован в СУБД TrizDB!", true);
					TextBoxQR.Text = ""; TextBoxInventoryNumber.Text = ""; TextBoxName.Text = ""; TextBoxLocation.Text = ""; TextBoxUsers.Text = ""; hfCachedPhotosBase64.Value = "";
					ddlResponsiblePerson.SelectedValue = "0";
					SetFormMode(false, "", false);
					TextBoxQR.Style["border"] = "1px solid #ccc";
					Session[KeyFormData] = null;
				}
				else throw new Exception(response);
			}
			catch (Exception ex) { ShowInformer("Ошибка фиксации квантов: " + ex.Message, false); }
		}

		// ================= СЫЧЁВСКИЙ КЛАСС СОСТОЯНИЯ ФОРМЫ =================
		public class AssetCardFormData
		{
			public string QR { get; set; }
			public string InventoryNumber { get; set; }
			public string AssetsName { get; set; }
			public string Location { get; set; }
			public string Users { get; set; }
			public List<byte[]> RawPhotos { get; set; }

			public AssetCardFormData() { RawPhotos = new List<byte[]>(); }

			public void CopyFormDataToPage(AssetCard page)
			{
				page.TextBoxQR.Text = QR; page.TextBoxInventoryNumber.Text = InventoryNumber;
				page.TextBoxName.Text = AssetsName; page.TextBoxLocation.Text = Location; page.TextBoxUsers.Text = Users;
				if (RawPhotos != null && RawPhotos.Count > 0)
				{
					List<string> b64 = new List<string>();
					foreach (byte[] b in RawPhotos) b64.Add("data:image/jpeg;base64," + Convert.ToBase64String(b));
					page.hfCachedPhotosBase64.Value = string.Join("|||", b64.ToArray());
				}
			}
			public static AssetCardFormData CreateFromPage(AssetCard page)
			{
				AssetCardFormData r = new AssetCardFormData { QR = page.TextBoxQR.Text, InventoryNumber = page.TextBoxInventoryNumber.Text, AssetsName = page.TextBoxName.Text, Location = page.TextBoxLocation.Text, Users = page.TextBoxUsers.Text };
				string sData = page.hfCachedPhotosBase64.Value;
				if (!string.IsNullOrEmpty(sData))
				{
					string[] split = sData.Split(new string[] { "|||" }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string s in split)
					{
						try
						{
							string clean = s.Contains(",") ? s.Substring(s.IndexOf(",") + 1) : s;
							r.RawPhotos.Add(Convert.FromBase64String(clean));
						}
						catch { }
					}
				}
				return r;
			}
		}
	}
}