using System;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;

namespace WsFileSystemMonitor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                var servico = new FileSystemMonitorService();
                string parameter = string.Concat(args);
                switch (parameter)
                {
                    case "--install":
                        if (!IsInstalled("WsFileSystemMonitor"))
                            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                        break;
                    case "--uninstall":
                        if (IsInstalled("WsFileSystemMonitor"))
                            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                        break;
                    default:
                        servico.Start(args);
                        break;
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] {
                    new FileSystemMonitorService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }

        static bool IsInstalled(string serviceName)
        {
            ServiceController ctl = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == serviceName);
            return ctl != null;
        }
    }
}
