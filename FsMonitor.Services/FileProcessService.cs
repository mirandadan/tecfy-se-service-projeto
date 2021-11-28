using FsMonitor.CrossCutting.Http;
using FsMonitor.CrossCutting.Model;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FsMonitor.Services
{
    public class FileProcessService : IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly string directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        private readonly string
            _processingFolder = ConfigurationManager.AppSettings["ProcessingFolder"],
            _completeFolder = ConfigurationManager.AppSettings["CompleteFolder"],
            _errorFolder = ConfigurationManager.AppSettings["ErrorFolder"],
            _folderHash = ConfigurationManager.AppSettings["FolderHash"];
        private readonly bool _logUploadResult;
        private readonly int _lifeTime = 0;
        private readonly List<FolderConfig> _folderConfig;

        private readonly MonitorService _monitorService = new MonitorService();
        private readonly HttpSender _httpSender = new HttpSender(
            ConfigurationManager.AppSettings["BaseAddress"],
            ConfigurationManager.AppSettings["UserName"],
            ConfigurationManager.AppSettings["Password"]
        );

        private Task _scrollCompletedFiles;
        private bool _stop = false;

        public FileProcessService()
        {
            bool.TryParse(ConfigurationManager.AppSettings["LogUploadResult"], out _logUploadResult);
            int.TryParse(ConfigurationManager.AppSettings["ProcessedFileLifeTime"], out _lifeTime);

            var json = File.ReadAllText(Path.Combine(directory, "folderConfig.json"));
            _folderConfig = JsonConvert.DeserializeObject<List<FolderConfig>>(json);

            CreateDir(_processingFolder);
            CreateDir(_completeFolder);
            CreateDir(_errorFolder);
        }

        public void Start()
        {
            _monitorService.OnFileReady += Monitor_OnFileReady;
            _monitorService.Start();

            if (_lifeTime >= 0)
            {
                _scrollCompletedFiles = Task.Factory.StartNew(ScrollCompletedFiles);
            }
        }

        private void ScrollCompletedFiles()
        {
            while (!_stop)
            {
                string[] files = Directory.GetFiles(_completeFolder);
                logger.Info($"{files.Length} arquivos encontrados completados: {_completeFolder}");
                for (int i = 0; i < files.Length; i++)
                {
                    try
                    {
                        if (_lifeTime == 0)
                        {
                            logger.Info($"Arquivo deletado: {files[i]}");
                            File.Delete(files[i]);
                        }
                        else
                        {
                            var fileInfo = new FileInfo(files[i]);
                            if (fileInfo.CreationTime < DateTime.Now.AddMinutes(-1 * _lifeTime))
                            {
                                logger.Info($"Excluindo arquivo: {fileInfo} escrito pela última vez em {fileInfo.LastAccessTime}");
                                fileInfo.Delete();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e);
                    }
                }
                Task.Delay(30000).Wait();
            }
        }

        private void CreateDir(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                logger.Info($"Diretório criado: {dir}");
            }
        }
        private string Move(string fileName, string destinationFolder)
        {
            var folder = GetFolder(fileName);
            destinationFolder = Path.Combine(destinationFolder, folder);

            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            var destFileName = Path.Combine(destinationFolder, Path.GetFileName(fileName));

            logger.Info($"Movendo arquivo de {fileName} para {destFileName}");

            if (File.Exists(destFileName))
            {
                File.Delete(destFileName);
            }

            File.Move(fileName, destFileName);
            return destFileName;
        }
        private void Monitor_OnFileReady(object sender, MonitorEventArgs e)
        {
            logger.Info($"Arquivo a ser processado: {e.FullName}");
            var currentFileName = e.FullName;
            if (File.Exists(e.FullName))
            {
                if (!currentFileName.Contains(_processingFolder))
                {
                    currentFileName = Move(e.FullName, _processingFolder);
                }
                try
                {
                    Send(currentFileName);
                    Move(currentFileName, _completeFolder);
                }
                catch (Exception ex)
                {
                    Move(currentFileName, _errorFolder);
                    logger.Info($"Devido ao erro ao enviar o arquivo, está sendo movido {currentFileName} para {_errorFolder}");
                    logger.Error($"ERRO: {ex}");
                }
            }
        }
        private void Send(string currentFileName)
        {
            try
            {
                logger.Info($"Enviando: {currentFileName}");
                var buffer = File.ReadAllBytes(currentFileName);
                var jsonResult = _httpSender.SendFormData(buffer, Path.GetFileName(currentFileName), GetHash(currentFileName));
                logger.Info($"Enviado com sucesso arquivo:{currentFileName} resposta: {jsonResult}");
                SaveResultLog(currentFileName, jsonResult);
            }
            catch (WebException ex)
            {
                logger.Error($"Erro ao enviar aquivo Source: {ex.Source}");
            }
            catch (Exception ex)
            {
                logger.Error($"Erro ao enviar aquivo Source: {ex.Source}");
                throw ex;
            }
        }

        private void SaveResultLog(string currentFileName, string jsonResult)
        {
            if (_logUploadResult)
            {
                var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var logFolder = Path.Combine(currentFolder, "Logs", "UploadResults", DateTime.Today.ToString("yyyy-MM-dd"));
                if (!Directory.Exists(logFolder))
                {
                    Directory.CreateDirectory(logFolder);
                }

                var fileName = Path.Combine(logFolder, $"{Path.GetFileNameWithoutExtension(currentFileName)}_{DateTime.Now.Ticks}.json");
                File.WriteAllText(fileName, jsonResult, Encoding.Default);
            }
        }

        private string GetHash(string currentFileName)
        {
            var folder = GetFolder(currentFileName);

            return _folderConfig.Where(f => { return f.Folder == folder; }).First().Hash;
        }

        private string GetFolder(string currentFileName)
        {
            var folder = currentFileName.Split('\\');
            var directory = folder.Length - 2;

            return folder[directory];
        }
        public void Dispose()
        {
            _monitorService.Dispose();

            if (_scrollCompletedFiles != null)
            {
                _stop = true;
                _scrollCompletedFiles.Wait(3000);
                if (_scrollCompletedFiles.IsCompleted)
                    _scrollCompletedFiles.Dispose();
            }
        }
    }
}
