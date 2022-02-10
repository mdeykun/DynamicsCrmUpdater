using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrmWebResourcesUpdater.Common.Extensions;
using McTools.Xrm.Connection;
using System.Security.Cryptography;
using CrmWebResourcesUpdater.Common;
using Microsoft.VisualStudio.Shell;

namespace CrmWebResourcesUpdater
{

    /// <summary>
    /// Provides methods for loading and saving user settings
    /// </summary>
    public class Settings
    {
        const string CollectionBasePath = "CrmPublisherSettings";
        const string ConnectionsPropertyName = "Connections";
        const string SelectedConnectionIdPropertyName = "SelectedConnectionId";
        const string AutoPublishPropertyName = "AutoPublishEnabled";
        const string IgnoreExtensionsPropertyName = "IgnoreExtensions";
        const string ExtendedLogPropertyName = "ExtendedLog";
        const string SettingsVersionPropertyName = "SettingsVersion";

        public const string FileKindGuid =         "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";
        public const string ProjectKindGuid =      "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";
        public const string MappingFileName = "UploaderMapping.config";

        public const string CurrentConfigurationVersion = "1";

        private byte[] entropy = Encoding.Unicode.GetBytes("crm.publisher");
        private WritableSettingsStore _settingsStore;
        private Guid _projectGuid;
        

        /// <summary>
        /// Crm Connections
        /// </summary>
        public CrmConnections CrmConnections { get; set; }

        /// <summary>
        /// Selected Connection Guid
        /// </summary>
        public Guid? SelectedConnectionId { get; set; }

        /// <summary>
        /// Configuration version
        /// </summary>
        public string ConfigurationVersion { get; set; }

        /// <summary>
        /// Collection Path based on project guid
        /// </summary>
        public string CollectionPath
        {
            get
            {
                return CollectionBasePath + "_" + _projectGuid.ToString();
            }
        }

        /// <summary>
        /// Selected Connection used to publish web resources
        /// </summary>
        public ConnectionDetail SelectedConnection
        {
            get
            {
                //Load();
                if (CrmConnections == null || CrmConnections.Connections == null || SelectedConnectionId == null)
                {
                    return null;
                }
                return CrmConnections.Connections.Where(c => c.ConnectionId == SelectedConnectionId.Value).FirstOrDefault();
            }
            set
            {
                if(value == null)
                {
                    SelectedConnectionId = null;
                    return;
                }
                if(CrmConnections == null)
                {
                    CrmConnections = new CrmConnections() { Connections = new List<ConnectionDetail>() { value } };
                }
                if(CrmConnections.Connections == null)
                {
                    CrmConnections.Connections = new List<ConnectionDetail>() { value };
                }
                if(CrmConnections.Connections.Where(c => c.ConnectionId == value.ConnectionId).Count() == 0)
                {
                    CrmConnections.Connections.Add(value);
                }
                SelectedConnectionId = value.ConnectionId;
            }
        }

        /// <summary>
        /// Gets Settings Instance
        /// </summary>
        /// <param name="serviceProvider">Extension service provider</param>
        /// <param name="projectGuid">Guid of project to read settings of</param>
        public Settings(IAsyncServiceProvider serviceProvider, Guid projectGuid)
        {
            _projectGuid = projectGuid;
            _settingsStore = GetWritableSettingsStore(serviceProvider);


            if (_settingsStore.CollectionExists(CollectionPath))
            {
                UpdateSettings();
                Load();
            }
            else
            {
                _settingsStore.CreateCollection(CollectionPath);
                ConfigurationVersion = CurrentConfigurationVersion;
                Save();
            }
        }

        private void UpdateSettings()
        {
            string configVersion = null;
            if (_settingsStore.PropertyExists(CollectionPath, SettingsVersionPropertyName))
            {
                configVersion = _settingsStore.GetString(CollectionPath, SettingsVersionPropertyName);
            }
            if (configVersion == null)
            {
                try
                {
                    var connections = new List<ConnectionDetail>();
                    var deprecatedConnectionsXml = _settingsStore.GetString(CollectionPath, ConnectionsPropertyName);
                    var deprecatedConnections = (List<McTools.Xrm.Connection.Deprecated.ConnectionDetail>)XmlSerializerHelper.Deserialize(deprecatedConnectionsXml, typeof(List<McTools.Xrm.Connection.Deprecated.ConnectionDetail>));
                    foreach (var connection in deprecatedConnections)
                    {
                        connections.Add(UpdateConnection(connection));
                    }
                    var connectionsXml = XmlSerializerHelper.Serialize(connections);
                    _settingsStore.SetString(CollectionPath, ConnectionsPropertyName, connectionsXml);
                    _settingsStore.SetString(CollectionPath, SettingsVersionPropertyName, CurrentConfigurationVersion);
                }
                catch (Exception)
                {
                    try
                    {
                        GetCrmConnections();
                        ConfigurationVersion = CurrentConfigurationVersion;
                        _settingsStore.SetString(CollectionPath, SettingsVersionPropertyName, ConfigurationVersion);
                    }
                    catch (Exception ex2)
                    {
                        Logger.Write("Failed to convert connections: " + ex2.Message);
                    }
                }
            }
        }

        private ConnectionDetail UpdateConnection(McTools.Xrm.Connection.Deprecated.ConnectionDetail deprecatedConnection)
        {
            //deprecatedConnection.CrmTicket
            //deprecatedConnection.OrganizationMajorVersion
            //deprecatedConnection.OrganizationMinorVersion
            //deprecatedConnection.PublisherPrefix
            //deprecatedConnection.UseOnline
            //deprecatedConnection.UseOsdp
            //deprecatedConnection.UseSsl

            var connection = new ConnectionDetail()
            {
                AuthType = deprecatedConnection.AuthType,
                ConnectionId = deprecatedConnection.ConnectionId,
                ConnectionName = deprecatedConnection.ConnectionName,
                IsCustomAuth = deprecatedConnection.IsCustomAuth,
                IsFromSdkLoginCtrl = false,
                LastUsedOn = DateTime.MinValue,
                //NewAuthType = Microsoft.Xrm.Tooling.Connector.AuthenticationType.AD,
                Organization = deprecatedConnection.Organization,
                OrganizationDataServiceUrl = deprecatedConnection.OrganizationServiceUrl.Replace("Organization.svc", "OrganizationData.svc"), //TODO: May be wrong conversion
                OrganizationFriendlyName = deprecatedConnection.OrganizationFriendlyName,
                OrganizationServiceUrl = deprecatedConnection.OrganizationServiceUrl,
                OrganizationUrlName = deprecatedConnection.OrganizationUrlName,
                OrganizationVersion = deprecatedConnection.OrganizationVersion,
                OriginalUrl = deprecatedConnection.WebApplicationUrl, //TODO: May be wrong conversion
                SavePassword = deprecatedConnection.SavePassword,
                ServerName = deprecatedConnection.OrganizationUrlName + "." + deprecatedConnection.ServerName, //TODO: May be wrong conversion
                TenantId = Guid.Empty,
                Timeout = deprecatedConnection.Timeout,
                TimeoutTicks = deprecatedConnection.TimeoutTicks,
                UseIfd = deprecatedConnection.UseIfd,
                UseMfa = false, //TODO: May be wrong conversion
                UserDomain = deprecatedConnection.UserDomain,
                UserName = deprecatedConnection.UserName,
                WebApplicationUrl = deprecatedConnection.WebApplicationUrl,
                SelectedSolution = new SolutionDetail()
                {
                    FriendlyName = deprecatedConnection.SolutionFriendlyName,
                    PublisherPrefix = deprecatedConnection.PublisherPrefix,
                    SolutionId = new Guid(deprecatedConnection.SolutionId),
                    UniqueName = deprecatedConnection.Solution
                }
            };
            if(connection.SavePassword)
            {
                connection.SetPassword(DecryptString(deprecatedConnection.UserPassword));
                connection.UserPasswordEncrypted = EncryptString(connection.UserPasswordEncrypted);
            }

            return connection;
        }


        /// <summary>
        /// Gets settings store for current user
        /// </summary>
        /// <param name="serviceProvider">Extension service provider</param>
        /// <returns>Returns Instanse of Writtable Settings Store for current user</returns>
        private WritableSettingsStore GetWritableSettingsStore(IAsyncServiceProvider serviceProvider)
        {
            var shellSettingsManager = new ShellSettingsManager(serviceProvider as IServiceProvider);
            return shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
        }

        /// <summary>
        /// Reads settings from settings store
        /// </summary>
        private void Load()
        {
            CrmConnections = GetCrmConnections();
            SelectedConnectionId = _settingsStore.GetGuid(CollectionPath, SelectedConnectionIdPropertyName);
            ConfigurationVersion = _settingsStore.GetString(CollectionPath, SettingsVersionPropertyName);
        }

        /// <summary>
        /// Saves settings to settings store
        /// </summary>
        public void Save()
        {
            SetCrmConnections(CrmConnections);
            _settingsStore.SetGuid(CollectionPath, SelectedConnectionIdPropertyName, SelectedConnectionId);
            if (ConfigurationVersion != null)
            {
                _settingsStore.SetString(CollectionPath, SettingsVersionPropertyName, ConfigurationVersion);
            }
        }

        /// <summary>
        /// Reads and parses Crm Connections from settings store
        /// </summary>
        /// <returns>Returns Crm Connetions</returns>
        private CrmConnections GetCrmConnections()
        {
            if (_settingsStore.PropertyExists(CollectionPath, ConnectionsPropertyName)) {
                var connectionsXml = _settingsStore.GetString(CollectionPath, ConnectionsPropertyName);
                List<ConnectionDetail> connections;
                try
                {
                    connections = (List<ConnectionDetail>)XmlSerializerHelper.Deserialize(connectionsXml, typeof(List<ConnectionDetail>));
                    var crmConnections = new CrmConnections() { Connections = connections };
                    var publisAfterUpload = true;
                    if(_settingsStore.PropertyExists(CollectionPath, AutoPublishPropertyName))
                    {
                        publisAfterUpload = _settingsStore.GetBoolean(CollectionPath, AutoPublishPropertyName);
                    }
                    crmConnections.PublishAfterUpload = publisAfterUpload;

                    var ignoreExtensions = false;
                    if (_settingsStore.PropertyExists(CollectionPath, IgnoreExtensionsPropertyName))
                    {
                        ignoreExtensions = _settingsStore.GetBoolean(CollectionPath, IgnoreExtensionsPropertyName);
                    }
                    crmConnections.IgnoreExtensions = ignoreExtensions;


                    var extendedLog = false;
                    if (_settingsStore.PropertyExists(CollectionPath, ExtendedLogPropertyName))
                    {
                        extendedLog = _settingsStore.GetBoolean(CollectionPath, ExtendedLogPropertyName);
                    }
                    crmConnections.ExtendedLog = extendedLog;

                    foreach (var connection in crmConnections.Connections)
                    {
                        if (!String.IsNullOrEmpty(connection.UserPasswordEncrypted))
                        {
                            connection.UserPasswordEncrypted = DecryptString(connection.UserPasswordEncrypted);
                        }
                    }

                    return crmConnections;
                }
                catch (Exception)
                {
                    Logger.WriteLine("Failed to parse connection settings");
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Writes Crm Connection to settings store
        /// </summary>
        /// <param name="crmConnections">Crm Connections to write to settings store</param>
        private void SetCrmConnections(CrmConnections crmConnections)
        {
            if(crmConnections == null || crmConnections.Connections == null)
            {
                _settingsStore.DeletePropertyIfExists(CollectionPath, ConnectionsPropertyName);
                return;
            }
            Dictionary<Guid, string> passwordCache = new Dictionary<Guid, string>();
            foreach(var connection in crmConnections.Connections)
            {
                if (connection.ConnectionId != null && !passwordCache.ContainsKey(connection.ConnectionId.Value))
                {
                    passwordCache.Add(connection.ConnectionId.Value, connection.UserPasswordEncrypted);
                }

                if (!String.IsNullOrEmpty(connection.UserPasswordEncrypted) && connection.SavePassword)
                {
                    connection.UserPasswordEncrypted = EncryptString(connection.UserPasswordEncrypted);
                }
                else
                {
                    connection.UserPasswordEncrypted = null;
                }
            }

            var connectionsXml = XmlSerializerHelper.Serialize(crmConnections.Connections);
            _settingsStore.SetString(CollectionPath, ConnectionsPropertyName, connectionsXml);
            _settingsStore.SetBoolean(CollectionPath, AutoPublishPropertyName, crmConnections.PublishAfterUpload);
            _settingsStore.SetBoolean(CollectionPath, IgnoreExtensionsPropertyName, crmConnections.IgnoreExtensions);
            _settingsStore.SetBoolean(CollectionPath, ExtendedLogPropertyName, crmConnections.ExtendedLog);

            foreach (var connection in crmConnections.Connections)
            {
                if (connection.ConnectionId != null && passwordCache.ContainsKey(connection.ConnectionId.Value))
                {
                    connection.UserPasswordEncrypted = passwordCache[connection.ConnectionId.Value];
                }
            }
        }

        /// <summary>
        /// Encrypts string
        /// </summary>
        /// <param name="input">String to encrypt</param>
        /// <returns>Encrypted sting</returns>
        private string EncryptString(string input)
        {
            byte[] encryptedData = ProtectedData.Protect(
                Encoding.Unicode.GetBytes(input),
                entropy,
                DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// Decrypts string
        /// </summary>
        /// <param name="encryptedData">Encrypted string to decrypt</param>
        /// <returns>Decrypted string</returns>
        private string DecryptString(string encryptedData)
        {
            try
            {
                byte[] decryptedData =ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    entropy,
                    DataProtectionScope.CurrentUser);
                return Encoding.Unicode.GetString(decryptedData);
            }
            catch
            {
                return null;
            }
        }
    }
}
