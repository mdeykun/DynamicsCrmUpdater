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
        private readonly ILogger logger;


        private readonly Dictionary<Guid, ProjectConfig> settingsCache = new Dictionary<Guid, ProjectConfig>();

        public ConfigurationService(
            ILogger logger,
            WritableSettingsStore settingsStore,
            VsDteService vsDteService,
            ConfigsConversionService configsConversionService)
        {
            this.vsDteService = vsDteService;
            this.settingsStore = settingsStore;
            this.configsConversionService = configsConversionService;
            this.logger = logger;
        }

        /// <summary>
        /// Gets Publisher settings for selected project
        /// </summary>
        /// <returns>Returns settings for selected project</returns>
        public async Task<ProjectConfig> GetProjectConfigAsync()
        {
            var projectInfo = await vsDteService.GetSelectedProjectInfoAsync();
            if (projectInfo == null)
            {
                await logger.WriteLineAsync("Project is not selected or selected project can't be identified");
                return null;
            }

            return await GetProjectConfigAsync(projectInfo.Guid);
        }

        public async Task<ProjectConfig> GetProjectConfigAsync(Guid projectGuid)
        {
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