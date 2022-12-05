using Cwru.Common;
using Cwru.Common.Services;
using Cwru.Connection.Services;
using Cwru.CrmRequests.Client;
using Cwru.CrmRequests.Common.Contracts;
using Cwru.Publisher.Services;
using Cwru.VsExtension.Commands;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.IO;
using System.ServiceModel;

namespace Cwru.VsExtension
{
    internal static class Resolver
    {
        public static void Initialize(AsyncPackage package)
        {
            XmlSerializerService = new Lazy<XmlSerializerService>();

            CrmRequestsClient = new Lazy<ICrmRequests>(() => new CrmRequestsClient(
                new NetNamedPipeBinding()
                {
                    ReceiveTimeout = new TimeSpan(0, 10, 0),
                    SendTimeout = new TimeSpan(0, 10, 0),
                    MaxReceivedMessageSize = 102400000,
                    MaxBufferSize = 102400000,
                    MaxBufferPoolSize = 102400000
                },
                new EndpointAddress("net.pipe://localhost/CrmWebResourceUpdaterSvc")));

            SettingsStore = new Lazy<WritableSettingsStore>(() =>
            {
                var shellSettingsManager = new ShellSettingsManager(package);
                return shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            });

            ToolConfigurationService = new Lazy<ToolConfigurationService>(() => new ToolConfigurationService(SettingsStore.Value));
            ProjectConfigurationService = new Lazy<ProjectConfigurationService>(() => new ProjectConfigurationService(SettingsStore.Value, ConfigsConversionService.Value));

            Logger = new Lazy<Logger>(() =>
            {
                var toolConfig = ToolConfigurationService.Value.GetToolConfig();
                return new Logger(package, toolConfig);
            });
            VsDteService = new Lazy<VsDteService>(() => new VsDteService(package, Logger.Value));

            WebResourceTypesService = new Lazy<WebResourceTypesService>(() => new WebResourceTypesService(Logger.Value));
            CryptoService = new Lazy<CryptoService>(() => new CryptoService());
            SolutionsService = new Lazy<SolutionsService>(() => new SolutionsService(CrmRequestsClient.Value));

            ConfigsConversionService = new Lazy<ConfigsConversionService>(() => new ConfigsConversionService(Logger.Value, CryptoService.Value, XmlSerializerService.Value, SettingsStore.Value));


            MappingService = new Lazy<MappingService>(() => new MappingService(Logger.Value, VsDteService.Value));

            CreateWrService = new Lazy<CreateWrService>(() => new CreateWrService(
                Logger.Value,
                CrmRequestsClient.Value,
                MappingService.Value,
                SolutionsService.Value,
                WebResourceTypesService.Value,
                VsDteService.Value));

            UpdateWrService = new Lazy<UpdateWrService>(() => new UpdateWrService(
                Logger.Value,
                CrmRequestsClient.Value,
                MappingService.Value,
                SolutionsService.Value,
                VsDteService.Value,
                ProjectConfigurationService.Value));

            DownloadWrService = new Lazy<DownloadWrService>(() => new DownloadWrService(
                Logger.Value,
                CrmRequestsClient.Value,
                MappingService.Value,
                SolutionsService.Value,
                WebResourceTypesService.Value,
                VsDteService.Value));

            ConnectionService = new Lazy<ConnectionService>(() => new ConnectionService(
                Logger.Value,
                CrmRequestsClient.Value,
                SolutionsService.Value,
                VsDteService.Value,
                MappingService.Value,
                ToolConfigurationService.Value,
                ProjectConfigurationService.Value));

            WatchdogService = new Lazy<WatchdogService>(() =>
            {
                var assemblyPath = Path.GetDirectoryName(typeof(CrmWebResourcesUpdater).Assembly.Location);

                return new WatchdogService(
                    Logger.Value,
                    $@"{assemblyPath}\Cwru.ServiceConsole.exe",
                    "Cwru.ServiceConsole");
            });

            CreateWebResourceCommand = new Lazy<CreateWrCommand>(() => new CreateWrCommand(Logger.Value, ConnectionService.Value, CreateWrService.Value));
            UpdaterOptionsCommand = new Lazy<UpdaterOptionsCommand>(() => new UpdaterOptionsCommand(Logger.Value, ConnectionService.Value));
            UpdateSelectedWebResourcesCommand = new Lazy<UpdateSelectedWrCommand>(() => new UpdateSelectedWrCommand(Logger.Value, ConnectionService.Value, UpdateWrService.Value));
            UpdateWrEnvironmentsCommand = new Lazy<UpdateWrEnvironmentsCommand>(() => new UpdateWrEnvironmentsCommand(Logger.Value, ConnectionService.Value, UpdateWrService.Value));
            UpdateWebResourcesCommand = new Lazy<UpdateWrCommand>(() => new UpdateWrCommand(Logger.Value, ConnectionService.Value, UpdateWrService.Value));
            DownloadSelectedWrCommand = new Lazy<DownloadSelectedWrCommand>(() => new DownloadSelectedWrCommand(Logger.Value, ConnectionService.Value, DownloadWrService.Value));
            DownloadWrsCommand = new Lazy<DownloadWrsCommand>(() => new DownloadWrsCommand(Logger.Value, ConnectionService.Value, DownloadWrService.Value));
        }

        public static Lazy<WritableSettingsStore> SettingsStore { get; private set; }
        public static Lazy<XmlSerializerService> XmlSerializerService { get; private set; }
        public static Lazy<ICrmRequests> CrmRequestsClient { get; private set; }
        public static Lazy<VsDteService> VsDteService { get; private set; }
        public static Lazy<MappingService> MappingService { get; private set; }
        public static Lazy<ProjectConfigurationService> ProjectConfigurationService { get; private set; }
        public static Lazy<ToolConfigurationService> ToolConfigurationService { get; private set; }
        public static Lazy<ConfigsConversionService> ConfigsConversionService { get; private set; }
        public static Lazy<CreateWrService> CreateWrService { get; private set; }
        public static Lazy<UpdateWrService> UpdateWrService { get; private set; }
        public static Lazy<DownloadWrService> DownloadWrService { get; private set; }
        public static Lazy<ConnectionService> ConnectionService { get; private set; }
        public static Lazy<CryptoService> CryptoService { get; private set; }
        public static Lazy<SolutionsService> SolutionsService { get; private set; }
        public static Lazy<Logger> Logger { get; private set; }
        public static Lazy<WatchdogService> WatchdogService { get; private set; }
        public static Lazy<WebResourceTypesService> WebResourceTypesService { get; private set; }
        public static Lazy<CreateWrCommand> CreateWebResourceCommand { get; private set; }
        public static Lazy<UpdaterOptionsCommand> UpdaterOptionsCommand { get; private set; }
        public static Lazy<UpdateSelectedWrCommand> UpdateSelectedWebResourcesCommand { get; private set; }
        public static Lazy<UpdateWrCommand> UpdateWebResourcesCommand { get; private set; }
        public static Lazy<UpdateWrEnvironmentsCommand> UpdateWrEnvironmentsCommand { get; private set; }
        public static Lazy<DownloadSelectedWrCommand> DownloadSelectedWrCommand { get; private set; }
        public static Lazy<DownloadWrsCommand> DownloadWrsCommand { get; private set; }
    }
}