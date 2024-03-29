﻿using Cwru.VsExtension.Commands.Base;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace Cwru.VsExtension
{
    /// <summary>
    /// CrmWebResourceUpdater extension package class
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class CrmWebResourcesUpdater : AsyncPackage
    {
        public const string PackageGuidString = "944f3eda-3d74-49f0-a2d4-a25775f1ab36";
        public static readonly Guid Package = new Guid("944f3eda-3d74-49f0-a2d4-a25775f1ab36");
        public static readonly Guid ProjectCommandSet = new Guid("e51702bf-0cd0-413b-87ba-7d267eecc6c2");
        public static readonly Guid ItemCommandSet = new Guid("AE7DC0B9-634A-46DB-A008-D6D15DD325E0");
        public static readonly Guid FolderCommandSet = new Guid("18CFE3ED-8E6B-4BD0-BFE7-9AFF7BF02009");

        /// <summary>
        /// Initializes a new instance of the <see cref="CrmWebResourcesUpdater"/> class.
        /// </summary>
        public CrmWebResourcesUpdater()
        {
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            Resolver.Initialize(this);
            Resolver.WatchdogService.Value.Start();

            await AddCommandAsync(0x0100, () => Resolver.UpdateWebResourcesCommand.Value, ProjectCommandSet);
            await AddCommandAsync(0x0200, () => Resolver.UpdaterOptionsCommand.Value, ProjectCommandSet);
            await AddCommandAsync(0x0300, () => Resolver.UpdateSelectedWebResourcesCommand.Value, ItemCommandSet);
            await AddCommandAsync(0x0400, () => Resolver.CreateWebResourceCommand.Value, ItemCommandSet);
            await AddCommandAsync(0x0500, () => Resolver.DownloadSelectedWrCommand.Value, ItemCommandSet);
            await AddCommandAsync(0x0600, () => Resolver.DownloadWrsCommand.Value, ProjectCommandSet, FolderCommandSet);
            await AddCommandAsync(0x0700, () => Resolver.UpdateWrEnvironmentsCommand.Value, ItemCommandSet);

            await InitializeDTEAsync(cancellationToken);
        }

        private async Task AddCommandAsync(int commandId, Func<IBaseCommand> getCommand, params Guid[] commandSets)
        {
            try
            {
                var commandService = (await this.GetServiceAsync(typeof(IMenuCommandService))) as OleMenuCommandService;
                if (commandService != null)
                {
                    foreach (var commandSet in commandSets)
                    {
                        var menuCommandID = new CommandID(commandSet, commandId);
                        var menuItem = new MenuCommand(async (object sender, EventArgs e) =>
                        {
                            try
                            {
                                var command = getCommand();
                                if (command != null)
                                {
                                    await command.ExecuteAsync();
                                }
                            }
                            catch (Exception ex)
                            {
                                await Resolver.Logger.Value.WriteLineAsync(ex);
                            }
                        },
                        menuCommandID);
                        commandService.AddCommand(menuItem);
                    }
                }
                else
                {
                    await Resolver.Logger.Value.WriteLineAsync("Warning: IMenuCommandService service is null.");
                }
            }
            catch (Exception ex)
            {
                await Resolver.Logger.Value.WriteLineAsync(ex);
            }
        }

        private async Task InitializeDTEAsync(CancellationToken cancellationToken)
        {
            try
            {
                var dteObj = (EnvDTE80.DTE2)await this.GetServiceAsync(typeof(EnvDTE.DTE));
                if (dteObj == null)
                {
                    await Resolver.Logger.Value.WriteLineAsync("Warning: DTE service is null. Seems that VisualStudio is not fully initialized.");
                }
            }
            catch (Exception ex)
            {
                await Resolver.Logger.Value.WriteLineAsync(ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            Resolver.WatchdogService.Value.Shutdown();
            base.Dispose(disposing);
        }
    }
}
