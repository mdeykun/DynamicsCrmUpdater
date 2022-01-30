using CrmWebResourcesUpdater.Common.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmWebResourcesUpdater.Common.Services
{
    public class SettingsService
    {
        private IAsyncServiceProvider serviceProvider;
        private JoinableTaskFactory joinableTaskFactory;
        private ProjectHelper projectHelper;
        private Dictionary<Guid, Settings> settingsCache = new Dictionary<Guid, Settings>();
        public static SettingsService Instance
        {
            get;
            private set;
        }
        public static void Initialize(AsyncPackage asyncPackage)
        {
            Instance = new SettingsService(asyncPackage);
        }

        public SettingsService(AsyncPackage asyncPackage)
        {
            serviceProvider = asyncPackage;
            joinableTaskFactory = asyncPackage.JoinableTaskFactory;
            projectHelper = new ProjectHelper(asyncPackage);
        }

        /// <summary>
        /// Gets Publisher settings for selected project
        /// </summary>
        /// <returns>Returns settings for selected project</returns>
        public async Task<Settings> GetSettingsAsync()
        {
            var project = await projectHelper.GetSelectedProjectAsync();
            var guid = await projectHelper.GetProjectGuidAsync(project);
            await joinableTaskFactory.SwitchToMainThreadAsync();
            if (settingsCache.ContainsKey(guid))
            {
                return settingsCache[guid];
            }
            var settings = new Settings(serviceProvider, guid);
            settingsCache.Add(guid, settings);
            return settings;
        }
    }
}
