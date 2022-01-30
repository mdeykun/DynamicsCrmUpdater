using CrmWebResourcesUpdater.Service.Common.Interfaces;
using McTools.Xrm.Connection;
using McTools.Xrm.Connection.WinForms;
using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;

namespace CrmWebResourcesUpdater.ServiceClientTestApp
{
    public partial class Form1 : Form
    {
        ICrmWebResourcesUpdaterService client;
        public Form1()
        {
            InitializeComponent();
            var pipeFactory = new ChannelFactory<ICrmWebResourcesUpdaterService>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/CrmWebResourceUpdaterSvc"));
            client = pipeFactory.CreateChannel();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            var connectionsXml = File.ReadAllText("connections.xml");
            var crmConnections = (CrmConnections)XmlSerializerHelper.Deserialize(connectionsXml, typeof(CrmConnections));
            var connection = crmConnections.Connections.First();
            //var response = client.GetSolutionsList(connection);

            var selector = new ConnectionSelector(crmConnections, connection, false, false);
            selector.OnCreateMappingFile = () => {
                MessageBox.Show("UploaderMapping.config successfully created", "Microsoft Dynamics CRM Web Resources Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            selector.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var connectionsXml = File.ReadAllText("connections.xml");
            var crmConnections = (CrmConnections)XmlSerializerHelper.Deserialize(connectionsXml, typeof(CrmConnections));
            var connection = crmConnections.Connections.First();
        }
    }
}
