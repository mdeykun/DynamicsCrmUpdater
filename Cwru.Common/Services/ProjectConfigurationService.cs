using Cwru.Common.Config;
using Cwru.Common.JsonConverters;
using Microsoft.VisualStudio.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cwru.Common.Services
{
    public class ProjectConfigurationService
    {
        const string CollectionBasePath = "CrmPublisherSettings";
        const string ProjectConfigPropertyName = "ProjectConfig";
        const string SettingsVersionPropertyName = "SettingsVersion";

        public const string CurrentConfigurationVersion = "2";

        private readonly WritableSettingsStore settingsStore;
        private readonly ConfigsConversionService configsConversionService;

        private readonly Dictionary<Guid, ProjectConfig> settingsCache = new Dictionary<Guid, ProjectConfig>();

        public ProjectConfigurationService(
            WritableSettingsStore settingsStore,
            ConfigsConversionService configsConversionService)
        {
            this.settingsStore = settingsStore;
            this.configsConversionService = configsConversionService;
        }

        public async Task<ProjectConfig> GetProjectConfigAsync(Guid projectGuid)
        {
            var config = GetFromCache(projectGuid);
            if (config != null)
            {
                return config;
            }

            config = await LoadProjectConfigAsync(projectGuid);
            UpdateCache(config);
            return config;
        }

        public void SaveProjectConfig(ProjectConfig projectConfig)
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

        private async Task<ProjectConfig> LoadProjectConfigAsync(Guid projectGuid)
        {
            var collectionPath = GetCollectionPath(projectGuid);
            if (settingsStore.CollectionExists(collectionPath))
            {
                var convertedConfig = await configsConversionService.ConvertSettingsAsync(projectGuid);
                if (convertedConfig != null)
                {
                    SaveProjectConfig(convertedConfig);
                }

                var projectConfigJson = settingsStore.GetString(collectionPath, ProjectConfigPropertyName);
                return JsonConvert.DeserializeObject<ProjectConfig>(projectConfigJson, GetJsonSerializationSettings());
            }
            else
            {
                var projectConfig = new ProjectConfig()
                {
                    ProjectId = projectGuid,
                    Version = CurrentConfigurationVersion,
                };

                return projectConfig;
            }
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