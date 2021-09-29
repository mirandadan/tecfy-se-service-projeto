namespace FsMonitor.Services
{
    internal class MonitorEventArgs
    {
        public string FullName { get; private set; }

        public MonitorEventArgs(string fullName)
        {
            FullName = fullName;
        }
    }
}
