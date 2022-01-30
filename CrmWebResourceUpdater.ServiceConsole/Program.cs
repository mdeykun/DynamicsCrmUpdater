using CrmWebResourcesUpdater.Service;
using CrmWebResourcesUpdater.Service.Common.Interfaces;
using CrmWebResourcesUpdater.Service.SdkLogin;
using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace CrmWebResourcesUpdater.ServiceConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            //CrmWebResourcesUpdaterService.ShowTestDialog();
            var svc = new CrmWebResourcesUpdaterService();
            var host = new ServiceHost(svc, new Uri("net.pipe://localhost"));
            host.AddServiceEndpoint(typeof(ICrmWebResourcesUpdaterService), new NetNamedPipeBinding()
            {
                MaxBufferSize = 102400000,
                MaxReceivedMessageSize = 102400000,
                MaxBufferPoolSize = 102400000
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
