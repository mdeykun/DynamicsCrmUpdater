using Cwru.CrmRequests.Common.Contracts;
using Cwru.CrmRequests.Service;
using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Cwru.ServiceConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //DEPLOY: not for production use:
            //var processes = Process.GetProcessesByName("Cwru.ServiceConsole");
            //var processesToKill = processes.Where(p => p.Id != Process.GetCurrentProcess().Id).ToList();
            //foreach (var process in processesToKill)
            //{
            //    process.Kill();
            //}

            var svc = new CrmRequestsService();
            var host = new ServiceHost(svc, new Uri("net.pipe://localhost"));
            host.AddServiceEndpoint(typeof(ICrmRequests), new NetNamedPipeBinding()
            {
                MaxBufferSize = 102400000,
                MaxReceivedMessageSize = 102400000,
                MaxBufferPoolSize = 102400000,
            }, "CrmWebResourceUpdaterSvc");
            host.Open();

            var monitoringTask = Task.Run(async () =>
            {
                while (true)
                {
                    if (Process.GetProcessesByName("devenv").Any() == false)
                    {
                        Environment.Exit(0);
                        break;
                    }
                    await Task.Delay(100);
                }
            });
            Console.ReadLine();
        }
    }
}
