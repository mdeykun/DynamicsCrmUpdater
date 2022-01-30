using CrmWebResourceUpdater.Service;
using McTools.Xrm.Connection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CrmWebResourceUpdater.ClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new CrmWebResourceUpdaterClient();

            var connectionsXml = File.ReadAllText("connections.xml");
            var crmConnections = (CrmConnections)XmlSerializerHelper.Deserialize(connectionsXml, typeof(CrmConnections));
            var connection = crmConnections.Connections.First();
            var response = client.GetSolutionsList(connection);

            Console.ReadLine();
        }
    }
}
