using NLog;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.IO;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace FsMonitor.Services
{
    internal class MonitorService : IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ConcurrentDictionary<string, DateTime> _pendingFiles = new ConcurrentDictionary<string, DateTime>();
        private readonly FileSystemWatcher _watcher = new FileSystemWatcher();
        private readonly string _pendingFolder = ConfigurationManager.AppSettings["PendingFolder"];
        private readonly string _processingFolder = ConfigurationManager.AppSettings["ProcessingFolder"];
        private Task _scrollPendentFiles;
        private bool _stop = false;
        private int _fileReadyDelay = 0;

        public event EventHandler<MonitorEventArgs> OnFileReady;

        public MonitorService()
        {
            var config = ConfigurationManager.AppSettings["FileReadyDelay"];
            if (!int.TryParse(config, out _fileReadyDelay)) {
                _fileReadyDelay = 10;
            }
        }

        public void Start()
        {
            StartMonitor();
            Task.Delay(30000).Wait();
            _scrollPendentFiles = Task.Factory.StartNew(ScrollPendentFiles);
        }

        public void Dispose()
        {
            if (_watcher != null) {
                _watcher.Dispose();
            }
            if (_scrollPendentFiles != null) {
                _stop = true;
                _scrollPendentFiles.Wait(30000);
                _scrollPendentFiles.Dispose();
            }
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private void StartMonitor()
        {
            if (!Directory.Exists(_pendingFolder)) {
                Directory.CreateDirectory(_pendingFolder);
            }

            var files = Directory.GetFiles(_pendingFolder, "*", SearchOption.AllDirectories);
            logger.Info($"{files.Length} arquivos pendentes encontrados: {_pendingFolder}");
            for (int i = 0; i < files.Length; i++) {
                _pendingFiles.TryAdd(files[i], DateTime.Now);
            }

            /*Vendo se existe arquivo no Processing para reprocessar*/
            var filesProcessing = Directory.GetFiles(_processingFolder, "*", SearchOption.AllDirectories);
            logger.Info($"{filesProcessing.Length} arquivos encontrados para processar: {_processingFolder}");
            for (int i = 0; i < filesProcessing.Length; i++)
            {
                _pendingFiles.TryAdd(filesProcessing[i], DateTime.Now);
            }

            _watcher.Path = _pendingFolder;
            _watcher.IncludeSubdirectories = true;

            _watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            _watcher.Created += _watcher_CreatedOrChanged;
            _watcher.Changed += _watcher_CreatedOrChanged;
            _watcher.Renamed += _watcher_CreatedOrChanged;
            _watcher.Deleted += _watcher_Deleted;

            _watcher.EnableRaisingEvents = true;
        }
        private void ScrollPendentFiles()
        {
            while (!_stop) {
                //logger.Info($"{_pendingFiles.Keys} arquivos pendentes");
                if (OnFileReady != null) {
                    foreach (string key in _pendingFiles.Keys) {
                        if (_stop) {
                            break;
                        }

                        if (File.Exists(key)) {
                            DateTime lastWrite;
                            if (_pendingFiles.TryGetValue(key, out lastWrite) && lastWrite < DateTime.Now.AddSeconds(-1 * _fileReadyDelay)) {
                                OnFileReady.Invoke(this, new MonitorEventArgs(key));
                            }
                        }
                    }
                }
                Task.Delay(3000).Wait();

            }
        }

        private void _watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (_pendingFiles.ContainsKey(e.FullPath)) {
                DateTime lastWrite;
                _pendingFiles.TryRemove(e.FullPath, out lastWrite);
            }
        }

        private void _watcher_CreatedOrChanged(object sender, FileSystemEventArgs e)
        {
            bool isDir = (File.GetAttributes(e.FullPath) & FileAttributes.Directory) == FileAttributes.Directory;

            if (!isDir) {
                if (_pendingFiles.ContainsKey(e.FullPath)) {
                    logger.Info($"Atualizando a data de alteração do arquivo {e.FullPath} no monitoramento.");
                    _pendingFiles.AddOrUpdate(e.FullPath, DateTime.Now, (key, oldvalue) => oldvalue = DateTime.Now);
                } else {
                    logger.Info($"Adicionando o arquivo {e.FullPath} no monitoramento.");
                    _pendingFiles.TryAdd(e.FullPath, DateTime.Now);
                }
            }
        }
    }
}
