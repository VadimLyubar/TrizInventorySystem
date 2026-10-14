using System;
using System.Web.UI;
namespace FirstASP
{
	public partial class ScannerQR : System.Web.UI.Page
	{
		protected void btnProcessCode_Click(object sender, EventArgs e) { this.ReturnWebForm(hfScanResult.Value); }
		protected void btnCancel_Click(object sender, EventArgs e) { this.ReturnWebForm(null); }
	}
}