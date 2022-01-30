using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace CrmWebResourcesUpdater.Common.Services
{
    public class WatchdogService
    {
        private string path;
        private string appName;
        private bool shutdown = false;
        private Task monitoringTask;
        private int crashCount = 0;
        private int crashCountTreshold = 15;

        public WatchdogService(string path, string appName)
        {
            this.path = path;
            this.appName = appName;
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
            var start = new System.Diagnostics.ProcessStartInfo();
            start.FileName = path;
            start.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden; //Hides GUI
            start.CreateNoWindow = true; //Hides console

            Process.Start(start);
        }

        public void Processing()
        {
            monitoringTask = Task.Run(async () =>
            {
                while (shutdown == false)
                {
                    if (Process.GetProcessesByName(appName).Any() == false)
                    {
                        LaunchServiceProcess();
                        crashCount++;
                    }
                    if(crashCount > crashCountTreshold)
                    {
                        await Logger.WriteAsync("Failed to launch publisher service");
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
