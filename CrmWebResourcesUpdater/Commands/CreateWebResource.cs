using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using System.Windows.Forms;
using CrmWebResourcesUpdater.Common;
using CrmWebResourcesUpdater.Common.Services;

namespace CrmWebResourcesUpdater
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CreateWebResource: BaseCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0400;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateWebResources"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private CreateWebResource(AsyncPackage package): base(package, CommandId, ProjectGuids.ItemCommandSet) {}

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CreateWebResource Instance
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
            Instance = new CreateWebResource(package);
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
                        var result = await PublishService.Instance.ShowConfigurationDialogAsync(ConfigurationMode.Normal);
                        if (result != DialogResult.OK)
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
                    Logger.WriteLine("Error: Connection is not selected");
                    return;
                }
                if(settings.SelectedConnection.PasswordIsEmpty)
                {
                    var result = await PublishService.Instance.ShowUpdatePasswordDialogAsync();
                    if (result != DialogResult.OK)
                    {
                        return;
                    }
                }
                await PublishService.Instance.CreateWebResourceAsync();
            }
            catch (Exception ex)
            {
                Logger.Write("An error occured: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }
    }
}
