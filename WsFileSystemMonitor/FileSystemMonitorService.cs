using FsMonitor.Services;
using NLog;
using System;
using System.ServiceProcess;

namespace WsFileSystemMonitor
{
    public partial class FileSystemMonitorService : ServiceBase
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private FileProcessService _fileProcessService;

        public FileSystemMonitorService()
        {
            InitializeComponent();
        }

        public void Start(string[] args)
        {
            OnStart(args);
            while (true) ;
        }

        protected override void OnStart(string[] args)
        {
            try {
                _logger.Info("Serviço de Monitoramento e Transferência de Arquivos iniciado.");
                _fileProcessService = new FileProcessService();
                _fileProcessService.Start();
            } catch (Exception e) {
                _logger.Error(e);
                OnStop();
            }
        }

        protected override void OnStop()
        {
            _logger.Info("Serviço de Monitoramento e Transferência de Arquivos: finalização solicitada.");
            _fileProcessService.Dispose();
        }
    }
}
