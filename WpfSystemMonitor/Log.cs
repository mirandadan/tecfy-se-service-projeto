using System;

namespace WpfSystemMonitor
{
    public class Log
    {
        public DateTime Data { get; set; }
        public string LogLevel { get; set; }
        public string Mensagem { get; set; }
        public string Callsite { get; set; }
    }
}
