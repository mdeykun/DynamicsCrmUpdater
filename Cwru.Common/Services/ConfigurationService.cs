using Cwru.Common.Config;
using Cwru.Common.JsonConverters;
using Microsoft.VisualStudio.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cwru.Common.Services
{
    public class ConfigurationService
    {
        const string CollectionBasePath = "CrmPublisherSettings";
        const string ProjectConfigPropertyName = "ProjectConfig";
        const string SettingsVersionPropertyName = "SettingsVersion";

        public const string CurrentConfigurationVersion = "2";

        private readonly WritableSettingsStore settingsStore;
        private readonly VsDteService vsDteService;
        private readonly ConfigsConversionService configsConversionService;

        private readonly Dictionary<Guid, ProjectConfig> settingsCache = new Dictionary<Guid, ProjectConfig>();

        public ConfigurationService(
            WritableSettingsStore settingsStore,
            VsDteService vsDteService,
            ConfigsConversionService configsConversionService)
        {
            this.vsDteService = vsDteService;
            this.settingsStore = settingsStore;
            this.configsConversionService = configsConversionService;
        }

        /// <summary>
        /// Gets Publisher settings for selected project
        /// </summary>
        /// <returns>Returns settings for selected project</returns>
        public async Task<ProjectConfig> GetProjectConfigAsync()
        {
            var projectInfo = await vsDteService.GetSelectedProjectInfoAsync();
            var projectGuid = projectInfo.Guid;

            //if (firstTime)
            //{
            //    firstTime = false;
            //    var projectConfig = new ProjectConfig()
            //    {
            //        ProjectId = projectGuid,
            //        Version = CurrentConfigurationVersion
            //    };

            //    Save(projectConfig);
            //    return projectConfig;
            //}

            var config = GetFromCache(projectGuid);
            if (config != null)
            {
                return config;
            }

            config = await LoadProjectConfigAsync(projectGuid);
            UpdateCache(config);
            return config;
        }

        private async Task<ProjectConfig> LoadProjectConfigAsync(Guid projectGuid)
        {
            var collectionPath = GetCollectionPath(projectGuid);
            if (settingsStore.CollectionExists(collectionPath))
            {
                //configsConversionService.UploadOldConfig(projectGuid);
                var projectConfig = await configsConversionService.ConvertSettingsAsync(projectGuid);
                if (projectConfig != null)
                {
                    Save(projectConfig);
                }

                return Load(projectGuid);
            }
            else
            {
                var projectConfig = new ProjectConfig()
                {
                    ProjectId = projectGuid,
                    Version = CurrentConfigurationVersion
                };

                return projectConfig;
            }
        }

        /// <summary>
        /// Saves settings to settings store
        /// </summary>
        public void Save(ProjectConfig projectConfig)
        {
            UpdateCache(projectConfig);

            var collectionPath = GetCollectionPath(projectConfig.ProjectId);
            if (!settingsStore.CollectionExists(collectionPath))
            {
                settingsStore.CreateCollection(collectionPath);
            }


            var projectConfigClone = (ProjectConfig)projectConfig.Clone();

            projectConfigClone.Environments?.ForEach(e =>
            {
                if (e.SavePassword == false && e.IsUserProvidedConnectionString == false && e.ConnectionString != null)
                {
                    e.ConnectionString.Password = null;
                    e.ConnectionString.ClientSecret = null;
                }
            });

            var projectConfigJson = JsonConvert.SerializeObject(projectConfigClone, GetJsonSerializationSettings());

            settingsStore.SetString(collectionPath, ProjectConfigPropertyName, projectConfigJson);
            settingsStore.SetString(collectionPath, SettingsVersionPropertyName, projectConfigClone.Version);
        }

        public async Task<bool> IsExtendedLoggingAsync()
        {
            var projectConfig = await this.GetProjectConfigAsync();
            var extendedLog = projectConfig?.ExtendedLog ?? false;

            return extendedLog;
        }

        /// <summary>
        /// Reads settings from settings store
        /// </summary>
        private ProjectConfig Load(Guid projectId)
        {
            var collectionPath = GetCollectionPath(projectId);
            var projectConfigJson = settingsStore.GetString(collectionPath, ProjectConfigPropertyName);
            var projectConfig = JsonConvert.DeserializeObject<ProjectConfig>(projectConfigJson, GetJsonSerializationSettings());

            return projectConfig;
        }

        private JsonSerializerSettings GetJsonSerializationSettings()
        {
            return new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter>
                {
                    new SecureStringConverter(CurrentConfigurationVersion),
                    new AuthenticationTypeConverter()
                }
            };
        }

        ///// <summary>
        ///// Reads and parses Crm Connections from settings store
        ///// </summary>
        ///// <returns>Returns Crm Connetions</returns>
        //private CrmConnections GetCrmConnections(string collectionPath)
        //{
        //    if (settingsStore.PropertyExists(collectionPath, ConnectionsPropertyName))
        //    {
        //        var connectionsXml = settingsStore.GetString(collectionPath, ConnectionsPropertyName);
        //        List<ConnectionDetail> connections;
        //        try
        //        {
        //            connections = (List<ConnectionDetail>)xmlSerializerService.Deserialize(connectionsXml, typeof(List<ConnectionDetail>));
        //            var crmConnections = new CrmConnections() { Connections = connections };
        //            var publisAfterUpload = true;
        //            if (settingsStore.PropertyExists(collectionPath, AutoPublishPropertyName))
        //            {
        //                publisAfterUpload = settingsStore.GetBoolean(collectionPath, AutoPublishPropertyName);
        //            }
        //            crmConnections.PublishAfterUpload = publisAfterUpload;
        //
        //            var ignoreExtensions = false;
        //            if (settingsStore.PropertyExists(collectionPath, IgnoreExtensionsPropertyName))
        //            {
        //                ignoreExtensions = settingsStore.GetBoolean(collectionPath, IgnoreExtensionsPropertyName);
        //            }
        //            crmConnections.IgnoreExtensions = ignoreExtensions;
        //
        //
        //            var extendedLog = false;
        //            if (settingsStore.PropertyExists(collectionPath, ExtendedLogPropertyName))
        //            {
        //                extendedLog = settingsStore.GetBoolean(collectionPath, ExtendedLogPropertyName);
        //            }
        //            crmConnections.ExtendedLog = extendedLog;
        //
        //            foreach (var connection in crmConnections.Connections)
        //            {
        //                if (!String.IsNullOrEmpty(connection.UserPasswordEncrypted))
        //                {
        //                    connection.UserPasswordEncrypted = DecryptString(connection.UserPasswordEncrypted);
        //                }
        //            }
        //
        //            return crmConnections;
        //        }
        //        catch (Exception)
        //        {
        //            Logger.WriteLine("Failed to parse connection settings");
        //            return null;
        //        }
        //    }
        //    return null;
        //}

        ///// <summary>
        ///// Writes Crm Connection to settings store
        ///// </summary>
        ///// <param name="crmConnections">Crm Connections to write to settings store</param>
        //private void SetCrmConnections(Settings settings)
        //{
        //    var crmConnections = settings.CrmConnections;

        //    if (crmConnections == null || crmConnections.Connections == null)
        //    {
        //        settingsStore.DeletePropertyIfExists(settings.CollectionPath, ConnectionsPropertyName);
        //        return;
        //    }
        //    Dictionary<Guid, string> passwordCache = new Dictionary<Guid, string>();
        //    foreach (var connection in crmConnections.Connections)
        //    {
        //        if (connection.ConnectionId != null && !passwordCache.ContainsKey(connection.ConnectionId.Value))
        //        {
        //            passwordCache.Add(connection.ConnectionId.Value, connection.UserPasswordEncrypted);
        //        }

        //        if (!String.IsNullOrEmpty(connection.UserPasswordEncrypted) && connection.SavePassword)
        //        {
        //            connection.UserPasswordEncrypted = EncryptString(connection.UserPasswordEncrypted);
        //        }
        //        else
        //        {
        //            connection.UserPasswordEncrypted = null;
        //        }
        //    }

        //    var connectionsXml = xmlSerializerService.Serialize(crmConnections.Connections);
        //    settingsStore.SetString(settings.CollectionPath, ConnectionsPropertyName, connectionsXml);
        //    settingsStore.SetBoolean(settings.CollectionPath, AutoPublishPropertyName, crmConnections.PublishAfterUpload);
        //    settingsStore.SetBoolean(settings.CollectionPath, IgnoreExtensionsPropertyName, crmConnections.IgnoreExtensions);
        //    settingsStore.SetBoolean(settings.CollectionPath, ExtendedLogPropertyName, crmConnections.ExtendedLog);

        //    foreach (var connection in crmConnections.Connections)
        //    {
        //        if (connection.ConnectionId != null && passwordCache.ContainsKey(connection.ConnectionId.Value))
        //        {
        //            connection.UserPasswordEncrypted = passwordCache[connection.ConnectionId.Value];
        //        }
        //    }
        //}

        private ProjectConfig GetFromCache(Guid projectGuid)
        {
            if (settingsCache.ContainsKey(projectGuid))
            {
                return settingsCache[projectGuid];
            }

            return null;
        }

        private void UpdateCache(ProjectConfig projectConfig)
        {
            if (!settingsCache.ContainsKey(projectConfig.ProjectId))
            {
                settingsCache.Add(projectConfig.ProjectId, projectConfig);
            }
        }

        private string GetCollectionPath(Guid projectId)
        {
            return CollectionBasePath + "_" + projectId.ToString();
        }
    }
}