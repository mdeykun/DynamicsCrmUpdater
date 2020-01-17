using System.Collections.Generic;

namespace McTools.Xrm.Connection
{
    /// <summary>
    /// Stores the list of Crm connections
    /// </summary>
    public class CrmConnections
    {
        #region Variables

        List<ConnectionDetail> _connections;

        string _proxyAddress;

        string _proxyPort;

        string _userName;

        string _password;

        bool _useCustomProxy;

        bool _publishAfterUpload = true;

        bool _ignoreExtensions = false;

        bool _extendedLog = false;

        #endregion

        #region Properties


        public List<ConnectionDetail> Connections
        {
            get { return _connections; }
            set { _connections = value; }
        }

        public string ProxyAddress
        {
            get { return _proxyAddress; }
            set { _proxyAddress = value; }
        }


        public string ProxyPort
        {
            get { return _proxyPort; }
            set { _proxyPort = value; }
        }


        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }


        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }


        public bool UseCustomProxy
        {
            get { return _useCustomProxy; }
            set { _useCustomProxy = value; }
        }

        public bool PublishAfterUpload { get { return _publishAfterUpload; } set { _publishAfterUpload = value; } }
        public bool IgnoreExtensions { get { return _ignoreExtensions; } set { _ignoreExtensions = value; } }
        public bool ExtendedLog { get { return _extendedLog; } set { _extendedLog = value; } }
        #endregion
    }
}