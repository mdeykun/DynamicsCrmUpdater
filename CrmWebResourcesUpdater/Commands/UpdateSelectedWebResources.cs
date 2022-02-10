using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using System.Windows.Forms;
using CrmWebResourcesUpdater.Common;
using CrmWebResourcesUpdater.Common.Services;
using CrmWebResourcesUpdater.DataModels;

namespace CrmWebResourcesUpdater
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class UpdateSelectedWebResources: BaseCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0300;


        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateSelectedWebResources"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private UpdateSelectedWebResources(AsyncPackage package): base(package, CommandId, ProjectGuids.ItemCommandSet)
        {
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static UpdateSelectedWebResources Instance
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
            Instance = new UpdateSelectedWebResources(package);
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
                if (settings.SelectedConnection.UseConnectionString == false)
                {
                    if (settings.SelectedConnection.NewAuthType == AuthenticationType.ClientSecret && settings.SelectedConnection.ClientSecretIsEmpty)
                    {
                        var result = await PublishService.Instance.ShowUpdateClientSecretDialogAsync();
                        if (result != DialogResult.OK)
                        {
                            return;
                        }
                    }
                    else if (settings.SelectedConnection.IsCustomAuth)
                    {
                        if (settings.SelectedConnection.PasswordIsEmpty && settings.SelectedConnection.Certificate == null)
                        {
                            var result = await PublishService.Instance.ShowUpdatePasswordDialogAsync();
                            if (result != DialogResult.OK)
                            {
                                return;
                            }
                        }
                    }
                }
                await this.package.JoinableTaskFactory.SwitchToMainThreadAsync();
                await PublishService.Instance.PublishWebResourcesAsync(true);
            }
            catch (Exception ex)
            {
                await Logger.WriteAsync("An error occured: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }
    }
}
