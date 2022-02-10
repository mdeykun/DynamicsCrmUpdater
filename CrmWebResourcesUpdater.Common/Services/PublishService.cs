using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using System.Threading.Tasks;
using CrmWebResourcesUpdater.Forms;
using System.Windows.Forms;
using System.Net;
using Microsoft.VisualStudio.Shell;
using CrmWebResourcesUpdater.Common.Helpers;
using CrmWebResourcesUpdater.DataModel;
using CrmWebResourcesUpdater.Service.Client;
using McTools.Xrm.Connection;
using McTools.Xrm.Connection.WinForms;
using Task = System.Threading.Tasks.Task;

namespace CrmWebResourcesUpdater.Common.Services
{
    /// <summary>
    /// Provides methods for uploading and publishing web resources
    /// </summary>
    public class PublishService : IDisposable
    {
        //private const string FetchWebResourcesQueryTemplate = @"<fetch mapping='logical' count='5000' version='1.0'>
        //                <entity name='webresource'>
        //                    <attribute name='name' />
        //                    <attribute name='content' />
        //                    <link-entity name='solutioncomponent' from='objectid' to='webresourceid'>
        //                        <filter>
        //                            <condition attribute='solutionid' operator='eq' value='{0}' />
        //                        </filter>
        //                    </link-entity>
        //                    {1}
        //                </entity>
        //            </fetch>";

        private ProjectHelper projectHelper = null;
        private MappingHelper mappingHelper = null;
        private AsyncPackage asyncPackage = null;
        private CrmWebResourcesUpdaterClient crmWebResourceUpdaterClient = null;

        //private ConnectionDetail _connectionDetail = null;
        //private bool autoPublish = true;
        //private bool ignoreExtensions = false;
        //private bool extendedLog = false;

        //private IOrganizationService _orgService = null;


        public static PublishService Instance
        {
            get;
            private set;
        }
        public static void Initialize(AsyncPackage asyncPackage)
        {
            Instance = new PublishService(asyncPackage);
        }


        /// <summary>
        /// Publisher constructor
        /// </summary>
        /// <param name="connection">Connection to CRM that will be used to upload webresources</param>
        /// <param name="autoPublish">Perform publishing or not</param>
        /// <param name="ignoreExtensions">Try to upload without extension if item not found with it</param>
        /// <param name="extendedLog">Print extended uploading process information</param>
        private PublishService(AsyncPackage asyncPackage)
        {
            projectHelper = new ProjectHelper(asyncPackage);
            mappingHelper = new MappingHelper(asyncPackage);
            this.asyncPackage = asyncPackage;

            crmWebResourceUpdaterClient = CrmWebResourcesUpdaterClient.Instance;
        }

        /// <summary>
        /// Uploads and publishes files to CRM
        /// </summary>
        public async System.Threading.Tasks.Task PublishWebResourcesAsync(bool uploadSelectedItems)
        {
            var settings = await SettingsService.Instance.GetSettingsAsync();
            var connections = settings.CrmConnections;
            var connectionDetail = settings.SelectedConnection;

            await projectHelper.SaveAllAsync();
            await Logger.ClearAsync();

            await projectHelper.SetStatusBarAsync("Uploading...");
            
            if (connections.PublishAfterUpload)
            {
                await Logger.WriteLineWithTimeAsync("Publishing web resources...");
            }
            else
            {
                await Logger.WriteLineWithTimeAsync("Uploading web resources...");
            }

            await Logger.WriteLineAsync("Connecting to CRM...");
            await Logger.WriteLineAsync("URL: " + connectionDetail.WebApplicationUrl);
            await Logger.WriteLineAsync("Solution Name: " + connectionDetail.SelectedSolution.FriendlyName);
            await Logger.WriteLineAsync("--------------------------------------------------------------");

            await Logger.WriteLineAsync("Loading files' paths", connections.ExtendedLog);
            var selectedFiles = await GetSelectedFilesAsync(uploadSelectedItems); //TODO: FIX PARAM
            if (selectedFiles == null || selectedFiles.Count == 0)
            {
                await Logger.WriteLineAsync("Failed to load files' paths", connections.ExtendedLog);
                return;
            }

            await Logger.WriteLineAsync(selectedFiles.Count + " path" + (selectedFiles.Count == 1 ? " was" : "s were") + " loaded", connections.ExtendedLog);
            try
            {
                await Logger.WriteLineAsync("Starting uploading process", connections.ExtendedLog);
                var webresources = await UploadWebResourcesAsync(selectedFiles);
                await Logger.WriteLineAsync("Uploading process was finished", connections.ExtendedLog);

                if (webresources.Count > 0)
                {
                    await Logger.WriteLineAsync("--------------------------------------------------------------");
                    foreach (var name in webresources.Values)
                    {
                        await Logger.WriteLineAsync(name + " successfully uploaded");
                    }
                }
                await Logger.WriteLineAsync("--------------------------------------------------------------");
                await Logger.WriteLineWithTimeAsync(webresources.Count + " file" + (webresources.Count == 1 ? " was" : "s were") + " uploaded");

                if (webresources.Count > 0 && connections.PublishAfterUpload)
                {
                    await projectHelper.SetStatusBarAsync("Publishing...");
                    await PublishWebResourcesAsync(webresources.Keys);
                }

                if (connections.PublishAfterUpload)
                {
                    await projectHelper.SetStatusBarAsync(webresources.Count + " web resource" + (webresources.Count == 1 ? " was" : "s were") + " published");
                }
                else
                {
                    await projectHelper.SetStatusBarAsync(webresources.Count + " web resource" + (webresources.Count == 1 ? " was" : "s were") + " uploaded");
                }

            }
            catch (Exception ex)
            {
                await projectHelper.SetStatusBarAsync("Failed to publish script" + (selectedFiles.Count == 1 ? "" : "s"));
                await Logger.WriteLineAsync("Failed to publish script" + (selectedFiles.Count == 1 ? "." : "s."));
                await Logger.WriteLineAsync(ex.Message);
                await Logger.WriteLineAsync(ex.StackTrace, connections.ExtendedLog);
            }
            await Logger.WriteLineWithTimeAsync("Done.");
        }


        /// <summary>
        /// Dispose object
        /// </summary>
        public void Dispose()
        {
           
        }


        public async Task<List<string>> GetSelectedFilesAsync(bool uploadSelectedItems)
        {
            var settings = await SettingsService.Instance.GetSettingsAsync();
            var connections = settings.CrmConnections;

            List<string> projectFiles = null;
            if (uploadSelectedItems)
            {
                await Logger.WriteLineAsync("Loading selected files' paths", connections.ExtendedLog);
                projectFiles = await projectHelper.GetSelectedFilesAsync();
            }
            else
            {
                await Logger.WriteLineAsync("Loading all files' paths", connections.ExtendedLog);
                projectFiles = await projectHelper.GetProjectFilesAsync();
            }

            return projectFiles;
        }

        /// <summary>
        /// Uploads web resources
        /// </summary>
        /// <returns>List of guids of web resources that was updated</returns>
        private async Task<Dictionary<Guid, string>> UploadWebResourcesAsync()
        {
            var projectFiles = await GetSelectedFilesAsync(true); //TODO: FIX PARAM

            if (projectFiles == null || projectFiles.Count == 0)
            {
                return null;
            }

            return await UploadWebResourcesAsync(projectFiles);
        }

        /// <summary>
        /// Uploads web resources
        /// </summary>
        /// <param name="selectedFiles"></param>
        /// <returns>List of guids of web resources that was updateds</returns>            
        private async Task<Dictionary<Guid, string>> UploadWebResourcesAsync(List<string> selectedFiles)
        {
            var settings = await SettingsService.Instance.GetSettingsAsync();
            var connections = settings.CrmConnections;

            var ids = new Dictionary<Guid, string>();

            var project = await projectHelper.GetSelectedProjectAsync();
            var projectRootPath = await projectHelper.GetProjectRootAsync(project);
            var mappings = await mappingHelper.LoadMappingsAsync(project);

            var filters = new List<string>();
            foreach (var filePath in selectedFiles)
            {
                var webResourceName = Path.GetFileName(filePath);
                var lowerFilePath = filePath.ToLower();

                if (webResourceName.ToLower() == Settings.MappingFileName.ToLower())
                {
                    continue;
                }

                if (mappings != null && mappings.ContainsKey(lowerFilePath))
                {
                    webResourceName = mappings[lowerFilePath];
                }
                else if (settings.CrmConnections.IgnoreExtensions)
                {
                    filters.Add(Path.GetFileNameWithoutExtension(filePath));
                }
                filters.Add(webResourceName);
            }
            var webResources = await RetrieveWebResourcesAsync(filters);

            foreach (var filePath in selectedFiles)
            {
                var webResourceName = Path.GetFileName(filePath);
                var lowerFilePath = filePath.ToLower();

                if (webResourceName.ToLower() == Settings.MappingFileName.ToLower())
                {
                    continue;
                }

                if (mappings != null && mappings.ContainsKey(lowerFilePath))
                {
                    webResourceName = mappings[lowerFilePath];
                    var relativePath = lowerFilePath.Replace(projectRootPath + "\\", "");
                    await Logger.WriteLineAsync("Mapping found: " + relativePath + " to " + webResourceName, connections.ExtendedLog);
                }

                var webResource = webResources.FirstOrDefault(x => x.Name == webResourceName);
                if(webResource == null && connections.IgnoreExtensions)
                {
                    await Logger.WriteLineAsync(webResourceName + " does not exists in selected solution", connections.ExtendedLog);
                    webResourceName = Path.GetFileNameWithoutExtension(filePath);
                    await Logger.WriteLineAsync("Searching for " + webResourceName, connections.ExtendedLog);
                    webResource = webResources.FirstOrDefault(x => x.Name == webResourceName);
                }
                if (webResource == null)
                {
                    await Logger.WriteLineAsync("Uploading of " + webResourceName + " was skipped: web resource does not exists in selected solution", connections.ExtendedLog);
                    await Logger.WriteLineAsync(webResourceName + " does not exists in selected solution", !connections.ExtendedLog);
                    continue;
                }
                if(!File.Exists(lowerFilePath))
                {
                    await Logger.WriteLineAsync("Warning: File not found: " + lowerFilePath);
                    continue;
                }
                var isUpdated = await UpdateWebResourceByFileAsync(webResource, filePath);
                if (isUpdated)
                {
                    ids.Add(webResource.Id.Value, webResourceName);
                }
            }
            return ids;
        }

        

        /// <summary>
        /// Uploads web resource
        /// </summary>
        /// <param name="webResource">Web resource to be updated</param>
        /// <param name="filePath">File with a content to be set for web resource</param>
        /// <returns>Returns true if web resource is updated</returns>
        private async Task<bool> UpdateWebResourceByFileAsync(WebResource webResource, string filePath)
        {
            var settings = await SettingsService.Instance.GetSettingsAsync();
            var connections = settings.CrmConnections;

            var webResourceName = Path.GetFileName(filePath);
            await Logger.WriteLineAsync("Uploading " + webResourceName, connections.ExtendedLog);

            var project = await projectHelper.GetSelectedProjectAsync();
            var projectRootPath = await projectHelper.GetProjectRootAsync(project);

            var localContent = FileHelper.GetEncodedFileContent(filePath);
            var remoteContent = webResource.Content;
            if (remoteContent.Length != localContent.Length || remoteContent != localContent)
            {
                await UpdateWebResourceByContentAsync(webResource, localContent);
                var relativePath = filePath.Replace(projectRootPath + "\\", "");
                await Logger.WriteLineAsync(webResource.Name + " uploaded from " + relativePath, !connections.ExtendedLog);
                return true;
            }
            else
            {
                await Logger.WriteLineAsync("Uploading of " + webResourceName + " was skipped: there aren't any change in the web resource", connections.ExtendedLog);
                await Logger.WriteLineAsync(webResourceName + " has no changes", !connections.ExtendedLog);
                return false;
            }
        }

        /// <summary>
        /// Uploads web resource
        /// </summary>
        /// <param name="webResource">Web resource to be updated</param>
        /// <param name="content">Content to be set for web resource</param>
        private async System.Threading.Tasks.Task UpdateWebResourceByContentAsync(WebResource webResource, string content)
        {
            var settings = await SettingsService.Instance.GetSettingsAsync();
            var connections = settings.CrmConnections;
            var selectedConnection = settings.SelectedConnection;

            var name = webResource.Name;
            webResource.Content = content;
            await crmWebResourceUpdaterClient.UploadWebresourceAsync(selectedConnection, webResource);

            await Logger.WriteLineAsync(name + " was successfully uploaded", connections.ExtendedLog);
        }

        /// <summary>
        /// Retrieves web resources for selected items
        /// </summary>
        /// <returns>List of web resources</returns>
        private async System.Threading.Tasks.Task<IEnumerable<WebResource>> RetrieveWebResourcesAsync(List<string> webResourceNames = null)
        {
            var settings = await SettingsService.Instance.GetSettingsAsync();
            var connections = settings.CrmConnections;
            var selectedConnection = settings.SelectedConnection;

            await Logger.WriteLineAsync("Retrieving existing web resources", connections.ExtendedLog);
            var retrieveWebResourceResponse = await crmWebResourceUpdaterClient.RetrieveWebResourcesAsync(selectedConnection, webResourceNames);
            if(retrieveWebResourceResponse.IsSuccessful == false)
            {
                throw new Exception($"Failed to retrieve web resource: {retrieveWebResourceResponse.Error}");
            }
            return retrieveWebResourceResponse.Payload;
        }

        /// <summary>
        /// Publishes webresources changes
        /// </summary>
        /// <param name="webresourcesIds">List of webresource IDs to publish</param>
        private async System.Threading.Tasks.Task PublishWebResourcesAsync(IEnumerable<Guid> webresourcesIds)
        {
            var settings = await SettingsService.Instance.GetSettingsAsync();
            var selectedConnection = settings.SelectedConnection;

            await Logger.WriteLineWithTimeAsync("Publishing...");
            //await Logger.WriteLineAsync("Publishing...");

            if (webresourcesIds == null || !webresourcesIds.Any())
            {
                throw new ArgumentNullException("webresourcesId");
            }
            if (webresourcesIds.Any())
            {
                await crmWebResourceUpdaterClient.PublishWebResourcesAsync(selectedConnection, webresourcesIds);
            }
            var count = webresourcesIds.Count();
            await Logger.WriteLineWithTimeAsync(count + " file" + (count == 1 ? " was" : "s were") + " published");
            //await Logger.WriteLineAsync(count + " file" + (count == 1 ? " was" : "s were") + " published");
        }

        private void CreateWebResource(ConnectionDetail connectionDetail, WebResource webResource, string solution)
        {
            if (webResource == null)
            {
                throw new ArgumentNullException("Web resource can not be null");
            }
            crmWebResourceUpdaterClient.CreateWebresource(connectionDetail, webResource, solution);
        }

        public async System.Threading.Tasks.Task CreateWebResourceAsync()
        {
            var settings = await SettingsService.Instance.GetSettingsAsync();
            var selectedConnection = settings.SelectedConnection;

            string publisherPrefix = selectedConnection.SelectedSolution.PublisherPrefix;
            if (publisherPrefix == null)
            {
                var result = MessageBox.Show("Publisher prefix is not loaded. Do you want to load it from CRM?", "Prefix is missing", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    await LoadPrefixAsync();
                }
                
            }
            await OpenCreateWebResourceFormAsync();
        }

        public void TestDialog()
        {
            (new CreateWebResourceForm(null, null)).Show();
            //(new TestForm()).Show();
        }

        private async Task OpenCreateWebResourceFormAsync()
        {
            var settings = await SettingsService.Instance.GetSettingsAsync();
            var project = await projectHelper.GetSelectedProjectAsync();
            var path = await projectHelper.GetSelectedFilePathAsync();
            var dialog = new CreateWebResourceForm(path, settings.SelectedConnection.SelectedSolution.PublisherPrefix);

            dialog.OnCreate = async (WebResource webResource) =>
            {
                var connectionDetail = settings.SelectedConnection;
                if (connectionDetail.SelectedSolution.SolutionId == null)
                {
                    throw new ArgumentNullException("SolutionId");
                }
                WebRequest.GetSystemWebProxy();

                var webresourceName = webResource.Name;

                var isWebResourceExistsResponse = await crmWebResourceUpdaterClient.IsWebResourceExistsAsync(connectionDetail, webresourceName);
                if(isWebResourceExistsResponse.IsSuccessful == false)
                {
                    MessageBox.Show($"Failed to validate webresource existance: {isWebResourceExistsResponse.Error}", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (isWebResourceExistsResponse.Payload)
                {
                    MessageBox.Show("Webresource with name '" + webresourceName + "' already exist in CRM.", "Webresource already exists.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    var isMappingRequired = await mappingHelper.IsMappingRequiredAsync(project, path, webresourceName);
                    var isMappingFileReadOnly = await mappingHelper.IsMappingFileReadOnlyAsync(project);
                    if (isMappingRequired && isMappingFileReadOnly)
                    {
                        Cursor.Current = Cursors.Arrow;
                        var message = "Mapping record can't be created. File \"UploaderMapping.config\" is read-only. Do you want to proceed? \r\n\r\n" +
                                        "Schema name of the web resource you are creating is differ from the file name. " +
                                        "Because of that new mapping record has to be created in the file \"UploaderMapping.config\". " +
                                        "Unfortunately the file \"UploaderMapping.config\" is read-only (file might be under a source control), so mapping record cant be created. \r\n\r\n" +
                                        "Press OK to proceed without mapping record creation (You have to do that manually later). Press Cancel to fix problem and try later.";
                        var result = MessageBox.Show(message, "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                        if (result == DialogResult.Cancel)
                        {
                            return;
                        }
                    }
                    Cursor.Current = Cursors.WaitCursor;
                    if (isMappingRequired && !isMappingFileReadOnly)
                    {
                        await mappingHelper.CreateMappingAsync(project, path, webresourceName);
                    }
                    //TODO: Check solution name
                    this.CreateWebResource(connectionDetail, webResource, settings.SelectedConnection.SelectedSolution.UniqueName);
                    await Logger.WriteLineAsync("Webresource '" + webresourceName + "' was successfully created");
                    dialog.Close();
                }
                catch (Exception ex)
                {
                    Cursor.Current = Cursors.Arrow;
                    MessageBox.Show("An error occured during web resource creation: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    await Logger.WriteLineAsync($"An error occured during web resource creation: {ex}", true);
                }
            };
            dialog.ShowDialog();
        }

        private async System.Threading.Tasks.Task LoadPrefixAsync()
        {
            var settings = await SettingsService.Instance.GetSettingsAsync();
            await Logger.WriteLineAsync("Retrieving Publisher prefix");
            var solution = await RetrieveSolutionAsync();
            string prefix = solution.PublisherPrefix;
            await Logger.WriteLineAsync("Publisher prefix successfully retrieved");
            
            settings.SelectedConnection.SelectedSolution.PublisherPrefix = prefix;
            settings.Save();
        }

        //private void BwGetSolutionRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    if (e.Error != null)
        //    {
        //        string errorMessage = e.Error.Message;
        //        var ex = e.Error.InnerException;
        //        while (ex != null)
        //        {
        //            errorMessage += "\r\nInner Exception: " + ex.Message;
        //            ex = ex.InnerException;
        //        }
        //        MessageBox.Show("An error occured while retrieving publisher prefix: " + errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            var entity = e.Result as Entity;
        //            string prefix = entity.GetAttributeValue<AliasedValue>("publisher.customizationprefix") == null ? null : entity.GetAttributeValue<AliasedValue>("publisher.customizationprefix").Value.ToString();
        //            await Logger.WriteLineAsync("Publisher prefix successfully retrieved");
        //            _connectionDetail.PublisherPrefix = prefix;
        //            var settings = projectHelper.GetSettingsAsync().Result;
        //            settings.SelectedConnection.PublisherPrefix = prefix;
        //            settings.Save();
        //            OpenCreateWebResourceFormAsync();
        //        }
        //    }
        //}

        //private void BwGetSolutionDoWork(object sender, DoWorkEventArgs e)
        //{
        //    e.Result = RetrieveSolution();
        //}

        private async Task<SolutionDetail> RetrieveSolutionAsync()
        {
            var settings = await SettingsService.Instance.GetSettingsAsync();
            var selectedConnection = settings.SelectedConnection;

            var retrieveSolutionResponse = await crmWebResourceUpdaterClient.RetrieveSolutionAsync(selectedConnection, selectedConnection.SelectedSolution.SolutionId);

            if(retrieveSolutionResponse.IsSuccessful == false)
            {
                throw new Exception($"Failed to retrieve solution: {retrieveSolutionResponse.Error}");
            }

            return retrieveSolutionResponse.Payload;
        }

        /// <summary>
        /// Shows Configuration Dialog
        /// </summary>
        /// <param name="mode">Configuration mode for settings dialog</param>
        /// <param name="project">Project to manage configuration for</param>
        /// <returns>Returns result of a configuration dialog</returns>
        public async Task<DialogResult> ShowConfigurationDialogAsync(ConfigurationMode mode)
        {
            var settings = await SettingsService.Instance.GetSettingsAsync();
            var project = await projectHelper.GetSelectedProjectAsync();
            var crmConnections = settings.CrmConnections == null ? new CrmConnections() { Connections = new List<ConnectionDetail>() } : settings.CrmConnections;
            var selector = new ConnectionSelector(crmConnections, settings.SelectedConnection, false, mode == ConfigurationMode.Update);
            selector.OnCreateMappingFile = async () => {
                await mappingHelper.CreateMappingFileAsync(project);
                MessageBox.Show("UploaderMapping.config successfully created", "Microsoft Dynamics CRM Web Resources Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            selector.ShowDialog();
            settings.CrmConnections = selector.ConnectionList;
            if (selector.DialogResult == DialogResult.OK || selector.DialogResult == DialogResult.Yes)
            {
                settings.SelectedConnection = selector.SelectedConnection;
            }
            settings.Save();
            return selector.DialogResult;
        }

        public async Task<DialogResult> ShowUpdatePasswordDialogAsync()
        {
            var settings = await SettingsService.Instance.GetSettingsAsync();
            var detail = settings.SelectedConnection;
            var dialog = new PasswordForm(detail)
            {
                UserLogin = detail.UserName,
                UserDomain = detail.UserDomain,
            };
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                detail.SetPassword(dialog.UserPassword, false);
                if (dialog.SavePassword)
                {
                    detail.SavePassword = true;
                    settings.Save();
                }
            }
            return dialog.DialogResult;
        }

        public async Task<DialogResult> ShowUpdateClientSecretDialogAsync()
        {
            var settings = await SettingsService.Instance.GetSettingsAsync();
            var detail = settings.SelectedConnection;
            var dialog = new SecretForm(detail)
            {
                ClientId = detail.AzureAdAppId.ToString("B")
            };
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                detail.SetClientSecret(dialog.ClientSecret, false);
                if (dialog.SaveSecret)
                {
                    detail.SavePassword = true;
                    settings.Save();
                }
            }

            return dialog.DialogResult;
        }

    }
}

