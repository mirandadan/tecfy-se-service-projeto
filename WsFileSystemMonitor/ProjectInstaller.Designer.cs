namespace WsFileSystemMonitor
{
    partial class ProjectInstaller
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
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.fsProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.fsServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // fsProcessInstaller
            // 
            this.fsProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.fsProcessInstaller.Password = null;
            this.fsProcessInstaller.Username = null;
            // 
            // fsServiceInstaller
            // 
            this.fsServiceInstaller.Description = "Serviço de Monitoramento e Transferência de Arquivos V.1.0.6";
            this.fsServiceInstaller.DisplayName = "Tecfy2SE-SB Monitoramento e Transferência de Arquivos V.1.0.6";
            this.fsServiceInstaller.ServiceName = "WsFileSystemMonitor";
            this.fsServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.fsProcessInstaller,
            this.fsServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller fsProcessInstaller;
        private System.ServiceProcess.ServiceInstaller fsServiceInstaller;
    }
}