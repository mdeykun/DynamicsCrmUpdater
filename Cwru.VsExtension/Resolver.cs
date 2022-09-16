using Cwru.Common;
using Cwru.Common.Services;
using Cwru.Connection.Services;
using Cwru.CrmRequests.Client;
using Cwru.CrmRequests.Common.Contracts;
using Cwru.Publisher;
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
            Logger = new Lazy<Logger>(() => new Logger(package));

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

            VsDteService = new Lazy<VsDteService>(() => new VsDteService(package, Logger.Value));
            SettingsStore = new Lazy<WritableSettingsStore>(() =>
            {
                var shellSettingsManager = new ShellSettingsManager(package);
                return shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            });

            CryptoService = new Lazy<CryptoService>(() => new CryptoService());
            SolutionsService = new Lazy<SolutionsService>(() => new SolutionsService(CrmRequestsClient.Value));

            ConfigsConversionService = new Lazy<ConfigsConversionService>(() => new ConfigsConversionService(Logger.Value, CryptoService.Value, XmlSerializerService.Value, SettingsStore.Value));
            ConfigurationService = new Lazy<ConfigurationService>(() =>
            {
                return new ConfigurationService(Logger.Value, SettingsStore.Value, VsDteService.Value, ConfigsConversionService.Value);
            });

            MappingService = new Lazy<MappingService>(() => new MappingService(VsDteService.Value));

            PublishService = new Lazy<PublishService>(() => new PublishService(
                Logger.Value,
                CrmRequestsClient.Value,
                MappingService.Value,
                SolutionsService.Value,
                VsDteService.Value));

            ConnectionService = new Lazy<ConnectionService>(() => new ConnectionService(
                Logger.Value,
                CrmRequestsClient.Value,
                SolutionsService.Value,
                VsDteService.Value,
                MappingService.Value,
                ConfigurationService.Value));

            WatchdogService = new Lazy<WatchdogService>(() =>
            {
                var assemblyPath = Path.GetDirectoryName(typeof(CrmWebResourcesUpdater).Assembly.Location);

                return new WatchdogService(
                    Logger.Value,
                    $@"{assemblyPath}\Cwru.ServiceConsole.exe",
                    "Cwru.ServiceConsole");
            });

            CreateWebResourceCommand = new Lazy<CreateWrCommand>(() => new CreateWrCommand(Logger.Value, ConnectionService.Value, PublishService.Value));
            UpdaterOptionsCommand = new Lazy<UpdaterOptionsCommand>(() => new UpdaterOptionsCommand(Logger.Value, ConnectionService.Value));
            UpdateSelectedWebResourcesCommand = new Lazy<UpdateSelectedWrCommand>(() => new UpdateSelectedWrCommand(Logger.Value, ConnectionService.Value, PublishService.Value));
            UpdateWebResourcesCommand = new Lazy<UpdateWrCommand>(() => new UpdateWrCommand(Logger.Value, ConnectionService.Value, PublishService.Value));
            DownloadSelectedWrCommand = new Lazy<DownloadSelectedWrCommand>(() => new DownloadSelectedWrCommand(Logger.Value, ConnectionService.Value, PublishService.Value));
            DownloadWrsCommand = new Lazy<DownloadWrsCommand>(() => new DownloadWrsCommand(Logger.Value, ConnectionService.Value, PublishService.Value));
        }

        public static Lazy<WritableSettingsStore> SettingsStore { get; private set; }
        public static Lazy<XmlSerializerService> XmlSerializerService { get; private set; }
        public static Lazy<ICrmRequests> CrmRequestsClient { get; private set; }
        public static Lazy<VsDteService> VsDteService { get; private set; }
        public static Lazy<MappingService> MappingService { get; private set; }
        public static Lazy<ConfigurationService> ConfigurationService { get; private set; }
        public static Lazy<ConfigsConversionService> ConfigsConversionService { get; private set; }
        public static Lazy<PublishService> PublishService { get; private set; }
        public static Lazy<ConnectionService> ConnectionService { get; private set; }
        public static Lazy<CryptoService> CryptoService { get; private set; }
        public static Lazy<SolutionsService> SolutionsService { get; private set; }
        public static Lazy<Logger> Logger { get; private set; }
        public static Lazy<WatchdogService> WatchdogService { get; private set; }
        public static Lazy<CreateWrCommand> CreateWebResourceCommand { get; private set; }
        public static Lazy<UpdaterOptionsCommand> UpdaterOptionsCommand { get; private set; }
        public static Lazy<UpdateSelectedWrCommand> UpdateSelectedWebResourcesCommand { get; private set; }
        public static Lazy<UpdateWrCommand> UpdateWebResourcesCommand { get; private set; }
        public static Lazy<DownloadSelectedWrCommand> DownloadSelectedWrCommand { get; private set; }
        public static Lazy<DownloadWrsCommand> DownloadWrsCommand { get; private set; }
    }
}