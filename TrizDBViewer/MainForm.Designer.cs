namespace TrizDBViewer
{
	using System;
	using System.Windows.Forms;

	public partial class MainForm : Form
	{
		private void InitializeComponent()
		{
			this.SuspendLayout();
			this.Name = "MainForm";
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.ResumeLayout(false);
		}
	}
}