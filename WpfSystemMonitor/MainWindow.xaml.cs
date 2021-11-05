using NLog;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Threading;

namespace WpfSystemMonitor
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
            logger.Info("Iniciado monitoramento de logs");
            dispatcherTimer.Tick += new EventHandler(TaskRefresh);
            int timeProcessed = 1;
            int.TryParse(ConfigurationManager.AppSettings["ProcessedReadTimeSeconds"], out timeProcessed);
            TimeSpan ts = TimeSpan.FromSeconds(timeProcessed);
            dispatcherTimer.Interval = ts;
            dispatcherTimer.Start();

        }

        private void TaskRefresh(object sender, EventArgs e)
        {
            listView.DataContext = DB.GetLogs();

            listView.SelectedIndex = listView.Items.Count - 1;
            listView.ScrollIntoView(listView.SelectedItem);
        }

        private void btStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (btStartStop.Content.ToString() == "Parar")
            {
                dispatcherTimer.Stop();
                btStartStop.Content = "Iniciar";
            }
            else
            {
                dispatcherTimer.Start();
                btStartStop.Content = "Parar";
            }
        }

        private void btClearRecords_Click(object sender, RoutedEventArgs e)
        {
            listView.DataContext = DB.DeleteLogs();

            listView.SelectedIndex = listView.Items.Count - 1;
            listView.ScrollIntoView(listView.SelectedItem);
        }
    }
}
