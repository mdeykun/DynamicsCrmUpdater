using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace Cwru.Common.Services
{
    public class WatchdogService
    {
        private bool shutdown = false;
        private int crashCount = 0;
        private int crashCountTreshold = 15;

        private readonly string path;
        private readonly string appName;
        private readonly Logger logger;

        public WatchdogService(Logger logger, string path, string appName)
        {
            this.path = path;
            this.appName = appName;
            this.logger = logger;
        }

        public void Start()
        {
            if (Process.GetProcessesByName(appName).Any() == false)
            {
                LaunchServiceProcess();
            }
            Processing();
        }

        public void LaunchServiceProcess()
        {
            var start = new ProcessStartInfo();
            start.FileName = path;
            start.WindowStyle = ProcessWindowStyle.Hidden; //Hides GUI
            start.CreateNoWindow = true; //Hides console

            Process.Start(start);
        }

        public void Processing()
        {
            Task.Run(async () =>
            {
                while (shutdown == false)
                {
                    if (Process.GetProcessesByName(appName).Any() == false)
                    {
                        LaunchServiceProcess();
                        crashCount++;
                    }
                    if (crashCount > crashCountTreshold)
                    {
                        await logger.WriteAsync("Failed to launch publisher service");
                        shutdown = true;
                        break;
                    }
                    await Task.Delay(100);
                }

                var serviceProcesses = Process.GetProcessesByName(appName);
                if (serviceProcesses.Any())
                {
                    serviceProcesses.ToList().ForEach(p => p.Kill());
                }
            });
        }

        public void Shutdown()
        {
            shutdown = true;
        }
    }
}
