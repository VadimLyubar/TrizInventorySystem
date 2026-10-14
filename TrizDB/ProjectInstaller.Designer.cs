namespace TrizDB
{
	partial class ProjectInstaller
	{
		/// <summary>
		/// Требуется переменная конструктора.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Освободить все используемые ресурсы.
		/// </summary>
		/// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Код, автоматически созданный конструктором компонентов

		/// <summary>
		/// Обязательный метод для поддержки конструктора - не изменяйте
		/// содержимое данного метода при помощи редактора кода.
		/// </summary>
		private void InitializeComponent()
		{
			this.serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();
			this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
			// 
			// serviceInstaller1
			// 
			this.serviceInstaller1.Description = "TrizDB";
			this.serviceInstaller1.DisplayName = "TrizDB для инвентаризации";
			this.serviceInstaller1.ServiceName = "TrizDB";
			this.serviceInstaller1.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
			// 
			// serviceProcessInstaller1
			// 
			this.serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
			this.serviceProcessInstaller1.Password = null;
			this.serviceProcessInstaller1.Username = null;
			// 
			// ProjectInstaller
			// 
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceInstaller1,
            this.serviceProcessInstaller1});

		}

		#endregion

		private System.ServiceProcess.ServiceInstaller serviceInstaller1;
		private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
	}
}