using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using System.Windows.Forms;
using CrmWebResourcesUpdater.Common;
using McTools.Xrm.Connection.WinForms;
using McTools.Xrm.Connection;
using System.Collections.Generic;
using CrmWebResourcesUpdater.Common.Services;

namespace CrmWebResourcesUpdater
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class UpdateWebResources: BaseCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        private UpdateWebResources(AsyncPackage package): base(package, CommandId, ProjectGuids.ItemCommandSet, ProjectGuids.ProjectCommandSet) {}

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static UpdateWebResources Instance
        {
            get;
            private set;
        }


        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(AsyncPackage package)
        {
            Instance = new UpdateWebResources(package);
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
            try
            { 
                var settings = await SettingsService.Instance.GetSettingsAsync();
                if (settings.SelectedConnection == null)
                {
                    if (projectHelper.ShowErrorDialog() == DialogResult.Yes)
                    {
                        var result = await PublishService.Instance.ShowConfigurationDialogAsync(ConfigurationMode.Update);
                        if (result != DialogResult.Yes)
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                if (settings.SelectedConnection == null)
                {
                    await Logger.WriteLineAsync("Error: Connection is not selected");
                    return;
                }
                if (settings.SelectedConnection.PasswordIsEmpty)
                {
                    var result = await PublishService.Instance.ShowUpdatePasswordDialogAsync();
                    if (result != DialogResult.OK)
                    {
                        return;
                    }
                }

                await PublishService.Instance.PublishWebResourcesAsync(false);
            }
            catch (Exception ex)
            {
                await Logger.WriteAsync("An error occured: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }
    }
}
