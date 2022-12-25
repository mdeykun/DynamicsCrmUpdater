using Cwru.CrmRequests.Common.Contracts;
using Cwru.CrmRequests.Service;
using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Cwru.ServiceConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var tcs = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) =>
            {
                tcs.Cancel();
                e.Cancel = true;
            };

            KillOtherProcesses();
            RunCrmRequestsService();

            await MonitorDevEnvAsync(tcs.Token);
        }

        static async Task MonitorDevEnvAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (Process.GetProcessesByName("devenv").Any() == false)
                {
                    Environment.Exit(0);
                    break;
                }
                await Task.Delay(500);
            }
        }

        static void RunCrmRequestsService()
        {
            var svc = new CrmRequestsService();
            var host = new ServiceHost(svc, new Uri("net.pipe://localhost"));
            host.AddServiceEndpoint(typeof(ICrmRequests), new NetNamedPipeBinding()
            {
                MaxBufferSize = 102400000,
                MaxReceivedMessageSize = 102400000,
                MaxBufferPoolSize = 102400000,
            }, "CrmWebResourceUpdaterSvc");
            host.Open();
        }

        [Conditional("DEBUG")]
        static void KillOtherProcesses()
        {
            Console.WriteLine("Killing other instances of Cwru.ServiceConsole");

            //DEPLOY: not for production use:
            var processes = Process.GetProcessesByName("Cwru.ServiceConsole");
            var processesToKill = processes.Where(p => p.Id != Process.GetCurrentProcess().Id).ToList();
            foreach (var process in processesToKill)
            {
                process.Kill();
            }
        }
    }
}
