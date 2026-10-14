namespace TrizDBViewer
{
	partial class FormView
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ts = new System.Windows.Forms.ToolStrip();
			this.btnRefresh = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.btnMols = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.btnDelete = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.lblSearch = new System.Windows.Forms.ToolStripLabel();
			this.txtSearch = new System.Windows.Forms.ToolStripTextBox();
			this.split = new System.Windows.Forms.SplitContainer();
			this.dgv = new System.Windows.Forms.DataGridView();
			this.QrCodeText = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.InventoryNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.AssetsName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ResponsiblePersonFio = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Users = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.rightPanel = new System.Windows.Forms.TableLayoutPanel();
			this.flowPhotos = new System.Windows.Forms.FlowLayoutPanel();
			this.lstHistory = new System.Windows.Forms.ListBox();
			this.btnEditLoc = new System.Windows.Forms.Button();
			this.ts.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.split)).BeginInit();
			this.split.Panel1.SuspendLayout();
			this.split.Panel2.SuspendLayout();
			this.split.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
			this.rightPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// ts
			// 
			this.ts.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnRefresh,
            this.toolStripSeparator1,
            this.btnMols,
            this.toolStripSeparator2,
            this.btnDelete,
            this.toolStripSeparator3,
            this.lblSearch,
            this.txtSearch});
			this.ts.Location = new System.Drawing.Point(0, 0);
			this.ts.Name = "ts";
			this.ts.Size = new System.Drawing.Size(1134, 25);
			this.ts.TabIndex = 0;
			// 
			// btnRefresh
			// 
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new System.Drawing.Size(107, 22);
			this.btnRefresh.Text = "🔄 Обновить базу";
			this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// btnMols
			// 
			this.btnMols.Name = "btnMols";
			this.btnMols.Size = new System.Drawing.Size(125, 22);
			this.btnMols.Text = "👥 Справочник МОЛ";
			this.btnMols.Click += new System.EventHandler(this.btnMols_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// btnDelete
			// 
			this.btnDelete.ForeColor = System.Drawing.Color.Red;
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.Size = new System.Drawing.Size(179, 22);
			this.btnDelete.Text = "❌ Удалить выбранный объект";
			this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// lblSearch
			// 
			this.lblSearch.Name = "lblSearch";
			this.lblSearch.Size = new System.Drawing.Size(128, 22);
			this.lblSearch.Text = "       Быстрый фильтр: ";
			// 
			// txtSearch
			// 
			this.txtSearch.Name = "txtSearch";
			this.txtSearch.Size = new System.Drawing.Size(180, 25);
			this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
			// 
			// split
			// 
			this.split.Dock = System.Windows.Forms.DockStyle.Fill;
			this.split.Location = new System.Drawing.Point(0, 25);
			this.split.Name = "split";
			// 
			// split.Panel1
			// 
			this.split.Panel1.Controls.Add(this.dgv);
			// 
			// split.Panel2
			// 
			this.split.Panel2.Controls.Add(this.rightPanel);
			this.split.Size = new System.Drawing.Size(1134, 586);
			this.split.SplitterDistance = 795;
			this.split.TabIndex = 1;
			// 
			// dgv
			// 
			this.dgv.AllowUserToAddRows = false;
			this.dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.QrCodeText,
            this.InventoryNumber,
            this.AssetsName,
            this.ResponsiblePersonFio,
            this.Users});
			this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgv.Location = new System.Drawing.Point(0, 0);
			this.dgv.MultiSelect = false;
			this.dgv.Name = "dgv";
			this.dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgv.Size = new System.Drawing.Size(795, 586);
			this.dgv.TabIndex = 0;
			this.dgv.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_CellEndEdit);
			this.dgv.SelectionChanged += new System.EventHandler(this.dgv_SelectionChanged);
			// 
			// QrCodeText
			// 
			this.QrCodeText.DataPropertyName = "QrCodeText";
			this.QrCodeText.HeaderText = "QR-код";
			this.QrCodeText.Name = "QrCodeText";
			this.QrCodeText.ReadOnly = true;
			this.QrCodeText.Width = 80;
			// 
			// InventoryNumber
			// 
			this.InventoryNumber.DataPropertyName = "InventoryNumber";
			this.InventoryNumber.HeaderText = "Инвентарный №";
			this.InventoryNumber.Name = "InventoryNumber";
			this.InventoryNumber.Width = 120;
			// 
			// AssetsName
			// 
			this.AssetsName.DataPropertyName = "AssetsName";
			this.AssetsName.HeaderText = "Наименование объекта имущества";
			this.AssetsName.Name = "AssetsName";
			this.AssetsName.Width = 230;
			// 
			// ResponsiblePersonFio
			// 
			this.ResponsiblePersonFio.DataPropertyName = "ResponsiblePersonFio";
			this.ResponsiblePersonFio.HeaderText = "Материально ответственное лицо (МОЛ)";
			this.ResponsiblePersonFio.Name = "ResponsiblePersonFio";
			this.ResponsiblePersonFio.Width = 180;
			// 
			// Users
			// 
			this.Users.DataPropertyName = "Users";
			this.Users.HeaderText = "Пользователи объекта";
			this.Users.Name = "Users";
			this.Users.Width = 140;
			// 
			// rightPanel
			// 
			this.rightPanel.ColumnCount = 1;
			this.rightPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.rightPanel.Controls.Add(this.flowPhotos, 0, 0);
			this.rightPanel.Controls.Add(this.lstHistory, 0, 1);
			this.rightPanel.Controls.Add(this.btnEditLoc, 0, 2);
			this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rightPanel.Location = new System.Drawing.Point(0, 0);
			this.rightPanel.Name = "rightPanel";
			this.rightPanel.RowCount = 3;
			this.rightPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.rightPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.rightPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
			this.rightPanel.Size = new System.Drawing.Size(335, 586);
			this.rightPanel.TabIndex = 0;
			// 
			// flowPhotos
			// 
			this.flowPhotos.AutoScroll = true;
			this.flowPhotos.BackColor = System.Drawing.Color.White;
			this.flowPhotos.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowPhotos.Location = new System.Drawing.Point(3, 3);
			this.flowPhotos.Name = "flowPhotos";
			this.flowPhotos.Size = new System.Drawing.Size(329, 273);
			this.flowPhotos.TabIndex = 0;
			// 
			// lstHistory
			// 
			this.lstHistory.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstHistory.FormattingEnabled = true;
			this.lstHistory.HorizontalScrollbar = true;
			this.lstHistory.Location = new System.Drawing.Point(3, 282);
			this.lstHistory.Name = "lstHistory";
			this.lstHistory.Size = new System.Drawing.Size(329, 273);
			this.lstHistory.TabIndex = 1;
			// 
			// btnEditLoc
			// 
			this.btnEditLoc.Location = new System.Drawing.Point(3, 561);
			this.btnEditLoc.Name = "btnEditLoc";
			this.btnEditLoc.Size = new System.Drawing.Size(205, 22);
			this.btnEditLoc.TabIndex = 2;
			this.btnEditLoc.Text = "📝 Редактирование строки истории";
			this.btnEditLoc.UseVisualStyleBackColor = true;
			this.btnEditLoc.Click += new System.EventHandler(this.btnEditLoc_Click);
			// 
			// FormView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1134, 611);
			this.Controls.Add(this.split);
			this.Controls.Add(this.ts);
			this.Name = "FormView";
			this.Text = "Диспетчер учёта имущества";
			this.Load += new System.EventHandler(this.btnRefresh_Click);
			this.ts.ResumeLayout(false);
			this.ts.PerformLayout();
			this.split.Panel1.ResumeLayout(false);
			this.split.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.split)).EndInit();
			this.split.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
			this.rightPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip ts;
		private System.Windows.Forms.ToolStripButton btnRefresh;
		private System.Windows.Forms.ToolStripButton btnMols;
		private System.Windows.Forms.ToolStripButton btnDelete;
		private System.Windows.Forms.ToolStripLabel lblSearch;
		private System.Windows.Forms.ToolStripTextBox txtSearch;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.SplitContainer split;
		private System.Windows.Forms.DataGridView dgv;
		private System.Windows.Forms.DataGridViewTextBoxColumn QrCodeText;
		private System.Windows.Forms.DataGridViewTextBoxColumn InventoryNumber;
		private System.Windows.Forms.DataGridViewTextBoxColumn AssetsName;
		private System.Windows.Forms.DataGridViewTextBoxColumn ResponsiblePersonFio;
		private System.Windows.Forms.DataGridViewTextBoxColumn Users;
		private System.Windows.Forms.TableLayoutPanel rightPanel;
		private System.Windows.Forms.FlowLayoutPanel flowPhotos;
		private System.Windows.Forms.ListBox lstHistory;
		private System.Windows.Forms.Button btnEditLoc;

	}
}