using System;

namespace FirstASP
{
	public partial class DefaultMenu : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e) { }
		protected void btnGoCard_Click(object sender, EventArgs e) { Response.Redirect("AssetCard.aspx"); }
		protected void btnGoMove_Click(object sender, EventArgs e) { Response.Redirect("MoveAssets.aspx"); }
	}
}