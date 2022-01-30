using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using CrmWebResourcesUpdater.Common.Helpers;

namespace CrmWebResourcesUpdater
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal class BaseCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        private readonly int CommandId;

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        internal readonly AsyncPackage package;

        /// <summary>
        /// Helper class, not null.
        /// </summary>
        internal readonly ProjectHelper projectHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateWebResources"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandId">Command ID, not null.</param>
        public BaseCommand(AsyncPackage package, int commandId, params Guid[] commandSets)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;
            this.projectHelper = new ProjectHelper(package);

            OleMenuCommandService commandService = this.ServiceProvider.GetServiceAsync(typeof(IMenuCommandService)).Result as OleMenuCommandService;
            if (commandService != null)
            {
                foreach (var commandSet in commandSets)
                {
                    var menuCommandID = new CommandID(commandSet, commandId);
                    var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                    commandService.AddCommand(menuItem);
                }
            }
        }


        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }


        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        public virtual async void MenuItemCallback(object sender, EventArgs e) { }
    }
}
