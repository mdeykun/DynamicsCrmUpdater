using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using CrmWebResourcesUpdater.Common;
using CrmWebResourcesUpdater.Helpers;

namespace CrmWebResourcesUpdater
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class UpdaterOptions: BaseCommand
    {
        private const int CommandId = 0x0200;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdaterOptions"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private UpdaterOptions(AsyncPackage package): base(package, CommandId, ProjectGuids.ProjectCommandSet) { }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(AsyncPackage package)
        {
            Instance = new UpdaterOptions(package);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static UpdaterOptions Instance
        {
            get;
            internal set;
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        public override async void MenuItemCallback(object sender, EventArgs e)
        {
            await PublishService.Instance.ShowConfigurationDialogAsync(ConfigurationMode.Normal);
        }
    }
}
