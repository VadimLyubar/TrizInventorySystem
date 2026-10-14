namespace FirstASP
{
	using System;
	using System.Web.UI;

	public partial class MoveAssets : System.Web.UI.Page
	{
		private static string KeyMoveData = "MoveAssets_StateData";

		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				if (Session[KeyMoveData] != null)
				{
					string[] state = (string[])Session[KeyMoveData];
					txtMoveQR.Text = state[0]; txtNewLocation.Text = state[1];
				}
				object result = this.ResultWebForm(typeof(MoveAssets), typeof(ScannerQR));
				if (result != null)
				{
					txtMoveQR.Text = result.ToString();
					VerifyObjectInDatabase(txtMoveQR.Text);
				}
			}
		}

		protected void btnScanMove_Click(object sender, EventArgs e)
		{
			Session[KeyMoveData] = new string[] { txtMoveQR.Text, txtNewLocation.Text };
			this.CallWebForm(typeof(MoveAssets), typeof(ScannerQR));
		}

		private void VerifyObjectInDatabase(string qrCode)
		{
			try
			{
				SocketService socket = Application["NetworkSocket"] as SocketService;
				if (socket == null) throw new Exception("Служба сокетов оффлайн.");

				string response = socket.SendStringCommand("CHECK_EXISTS|" + qrCode);
				pnlInformer.Visible = true;

				if (response.StartsWith("FOUND"))
				{
					string[] parts = response.Split('|');
					string assetName = parts[1];
					string lastLocation = parts[5]; // Извлекаем последнее актуальное местоположение из графа

					lblMoveInformerText.Text = "✅ СУБД подтверждает наличие: " + assetName;
					lblMoveInformerText.Style["color"] = "#03543f";
					pnlInformer.Style["background-color"] = "#f3faf7";
					pnlInformer.Style["border-left"] = "5px solid #31c48d";
					pnlInformer.Style["border-color"] = "#def7ec";

					TextBoxCurrentLocation.Text = lastLocation; // Выводим в трехстрочное поле для чтения
					txtNewLocation.Enabled = true;
					btnSubmitMove.Enabled = true;
					btnSubmitMove.Style["background-color"] = "#16a34a";
				}
				else
				{
					lblMoveInformerText.Text = "⚠️ Внимание: Объект с QR-кодом " + qrCode + " не найден в СУБД!";
					lblMoveInformerText.Style["color"] = "#c81e1e";
					pnlInformer.Style["background-color"] = "#fdf2f2";
					pnlInformer.Style["border-left"] = "5px solid #f05252";
					pnlInformer.Style["border-color"] = "#f8b4b4";

					TextBoxCurrentLocation.Text = "Объект отсутствует в базе данных.";
					txtNewLocation.Enabled = false;
					btnSubmitMove.Enabled = false;
					btnSubmitMove.Style["background-color"] = "#4b5563";
				}
			}
			catch (Exception ex)
			{
				lblMoveInformerText.Text = "Ошибка сети СУБД: " + ex.Message;
				lblMoveInformerText.Style["color"] = "#c81e1e";
				pnlInformer.Style["background-color"] = "#fdf2f2";
				pnlInformer.Visible = true;
			}
		}

		protected void btnSubmitMove_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(txtNewLocation.Text.Trim())) return;
			try
			{
				SocketService socket = Application["NetworkSocket"] as SocketService;
				string response = socket.SendStringCommand(string.Format("MOVE_ASSET|{0}|{1}", txtMoveQR.Text, txtNewLocation.Text.Trim()));
				if (response == "SUCCESS")
				{
					lblMoveInformerText.Text = "✅ История перемещения успешно записана в СУБД TrizDB!";
					lblMoveInformerText.Style["color"] = "#03543f";
					pnlInformer.Style["background-color"] = "#f3faf7";
					pnlInformer.Style["border-left"] = "5px solid #31c48d";
					pnlInformer.Style["border-color"] = "#def7ec";

					txtMoveQR.Text = ""; TextBoxCurrentLocation.Text = ""; txtNewLocation.Text = "";
					txtNewLocation.Enabled = false; btnSubmitMove.Enabled = false;
					btnSubmitMove.Style["background-color"] = "#4b5563";
					Session[KeyMoveData] = null;
				}
			}
			catch (Exception ex) { lblMoveInformerText.Text = "Ошибка СУБД: " + ex.Message; }
		}
	}
}