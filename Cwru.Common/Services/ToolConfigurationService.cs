using Cwru.Common.Config;
using Microsoft.VisualStudio.Settings;
using Newtonsoft.Json;

namespace Cwru.Common.Services
{
    public class ToolConfigurationService
    {
        const string CollectionPath = "CrmPublisherSettings";
        const string ToolConfigPropertyName = "ToolConfig";

        private readonly WritableSettingsStore settingsStore;
        private ToolConfig toolConfig;

        public ToolConfigurationService(WritableSettingsStore settingsStore)
        {
            this.settingsStore = settingsStore;
        }

        public ToolConfig GetToolConfig()
        {
            if (toolConfig != null)
            {
                return toolConfig;
            }

            try
            {
                if (settingsStore.CollectionExists(CollectionPath))
                {
                    var json = settingsStore.GetString(CollectionPath, ToolConfigPropertyName);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        toolConfig = JsonConvert.DeserializeObject<ToolConfig>(json);
                    }
                }
            }
            catch { }

            if (toolConfig == null)
            {
                toolConfig = new ToolConfig()
                {
                    ExtendedLog = false
                };
            }

            return toolConfig;
        }

        /// <summary>
        /// Saves settings to settings store
        /// </summary>
        public void SaveToolConfig(ToolConfig toolConfig)
        {
            if (toolConfig == null)
            {
                return;
            }

            if (this.toolConfig == null)
            {
                this.toolConfig = toolConfig;
            }
            else
            {
                this.toolConfig.ExtendedLog = toolConfig.ExtendedLog;
            }

            if (!settingsStore.CollectionExists(CollectionPath))
            {
                settingsStore.CreateCollection(CollectionPath);
            }

            var json = JsonConvert.SerializeObject(this.toolConfig);
            settingsStore.SetString(CollectionPath, ToolConfigPropertyName, json);
        }
    }
}