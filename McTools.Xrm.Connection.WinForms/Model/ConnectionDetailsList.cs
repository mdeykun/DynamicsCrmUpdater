using System;
using System.Collections.Generic;

namespace McTools.Xrm.Connection.WinForms.Model
{
    public class ConnectionDetailsList
    {
        public ConnectionDetailsList()
        {
            Connections = new List<ConnectionDetail>();
        }

        public ConnectionDetailsList(IEnumerable<ConnectionDetail> connections)
        {
            Connections = new List<ConnectionDetail>();
            Connections.AddRange(connections);
            Connections.Sort();
        }

        public List<ConnectionDetail> Connections { get; private set; }

        public Guid? SelectedConnectionId { get; set; }
        public bool PublishAfterUpload { get; set; }
        public bool IgnoreExtensions { get; set; }
        public bool ExtendedLog { get; set; }
    }
}