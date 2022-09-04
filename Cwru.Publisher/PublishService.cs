using Cwru.Common;
using Cwru.Common.Config;
using Cwru.Common.Extensions;
using Cwru.Common.Model;
using Cwru.Common.Services;
using Cwru.CrmRequests.Common.Contracts;
using Cwru.Forms;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace Cwru.Publisher
{
    /// <summary>
    /// Provides methods for uploading and publishing web resources
    /// </summary>
    public class PublishService
    {
        private readonly VsDteService vsDteHelper;
        private readonly MappingService mappingHelper;
        private readonly ICrmRequests crmWebResourcesUpdaterClient;
        private readonly ConfigurationService settingsService;
        private readonly SolutionsService solutionsService;
        private readonly Logger logger;

        /// <summary>
        /// Publisher constructor
        /// </summary>
        /// <param name="connection">Connection to CRM that will be used to upload webresources</param>
        /// <param name="autoPublish">Perform publishing or not</param>
        /// <param name="ignoreExtensions">Try to upload without extension if item not found with it</param>
        /// <param name="extendedLog">Print extended uploading process information</param>
        public PublishService(
            Logger logger,
            ICrmRequests crmWebResourcesUpdaterClient,
            VsDteService vsDteHelper,
            MappingService mappingHelper,
            ConfigurationService settingsService,
            SolutionsService solutionsService)
        {
            this.crmWebResourcesUpdaterClient = crmWebResourcesUpdaterClient;
            this.vsDteHelper = vsDteHelper;
            this.mappingHelper = mappingHelper;
            this.settingsService = settingsService;
            this.solutionsService = solutionsService;
            this.logger = logger;
        }

        /// <summary>
        /// Uploads and publishes files to CRM
        /// </summary>
        public async Task PublishWebResourcesAsync(bool uploadSelectedItems)
        {
            var projectConfig = await settingsService.GetProjectConfigAsync();
            var selectedEnvironmentConfig = projectConfig.GetSelectedEnvironment();

            var extendedLog = projectConfig.ExtendedLog;
            var publishAfterUpload = projectConfig.PublishAfterUpload;

            await vsDteHelper.SaveAllAsync();
            await logger.ClearAsync();

            await vsDteHelper.SetStatusBarAsync("Uploading...");

            if (publishAfterUpload)
            {
                await logger.WriteLineWithTimeAsync("Publishing web resources...");
            }
            else
            {
                await logger.WriteLineWithTimeAsync("Uploading web resources...");
            }

            await logger.WriteLineAsync("Connecting to CRM...");
            await logger.WriteLineAsync("URL: " + selectedEnvironmentConfig.ConnectionString.ServiceUri);

            var selectedSolution = await solutionsService.GetSolutionDetailsAsync(selectedEnvironmentConfig);
            await logger.WriteLineAsync("Solution Name: " + selectedSolution.FriendlyName);
            await logger.WriteLineAsync("--------------------------------------------------------------");

            await logger.WriteLineAsync("Loading files' paths", extendedLog);
            var selectedFiles = await GetSelectedFilesAsync(uploadSelectedItems);
            if (selectedFiles == null || selectedFiles.Count() == 0)
            {
                await logger.WriteLineAsync("Failed to load files' paths", extendedLog);
                return;
            }

            await logger.WriteLineAsync(selectedFiles.Count() + " path" + (selectedFiles.Count() == 1 ? " was" : "s were") + " loaded", extendedLog);
            try
            {
                await logger.WriteLineAsync("Starting uploading process", extendedLog);
                var webresources = await UploadWebResourcesAsync(selectedFiles);
                await logger.WriteLineAsync("Uploading process was finished", extendedLog);

                if (webresources.Count > 0)
                {
                    await logger.WriteLineAsync("--------------------------------------------------------------");
                    foreach (var name in webresources.Values)
                    {
                        await logger.WriteLineAsync(name + " successfully uploaded");
                    }
                }
                await logger.WriteLineAsync("--------------------------------------------------------------");
                await logger.WriteLineWithTimeAsync(webresources.Count + " file" + (webresources.Count == 1 ? " was" : "s were") + " uploaded");

                if (webresources.Count > 0 && publishAfterUpload)
                {
                    await vsDteHelper.SetStatusBarAsync("Publishing...");
                    await PublishWebResourcesAsync(webresources.Keys);
                }

                if (publishAfterUpload)
                {
                    await vsDteHelper.SetStatusBarAsync(webresources.Count + " web resource" + (webresources.Count == 1 ? " was" : "s were") + " published");
                }
                else
                {
                    await vsDteHelper.SetStatusBarAsync(webresources.Count + " web resource" + (webresources.Count == 1 ? " was" : "s were") + " uploaded");
                }

            }
            catch (Exception ex)
            {
                await vsDteHelper.SetStatusBarAsync("Failed to publish script" + (selectedFiles.Count() == 1 ? "" : "s"));
                await logger.WriteLineAsync("Failed to publish script" + (selectedFiles.Count() == 1 ? "." : "s."));
                await logger.WriteLineAsync(ex.Message);
                await logger.WriteLineAsync(ex.StackTrace, extendedLog);
            }
            await logger.WriteLineWithTimeAsync("Done.");
        }

        public async Task<IEnumerable<string>> GetSelectedFilesAsync(bool uploadSelectedItems)
        {
            var projectConfig = await settingsService.GetProjectConfigAsync();
            var projectInfo = await vsDteHelper.GetSelectedProjectInfoAsync();

            if (uploadSelectedItems)
            {
                await logger.WriteLineAsync("Loading selected files' paths", projectConfig.ExtendedLog);
                return projectInfo.SelectedFiles;
            }
            else
            {
                await logger.WriteLineAsync("Loading all files' paths", projectConfig.ExtendedLog);
                return projectInfo.Files;
            }
        }

        /// <summary>
        /// Uploads web resources
        /// </summary>
        /// <returns>List of guids of web resources that was updated</returns>
        private async Task<Dictionary<Guid, string>> UploadWebResourcesAsync()
        {
            var projectFiles = await GetSelectedFilesAsync(true);

            if (projectFiles == null || projectFiles.Count() == 0)
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
        private async Task<Dictionary<Guid, string>> UploadWebResourcesAsync(IEnumerable<string> selectedFiles)
        {
            var projectConfig = await settingsService.GetProjectConfigAsync();

            var ids = new Dictionary<Guid, string>();

            var projectInfo = await vsDteHelper.GetSelectedProjectInfoAsync();
            var mappings = await mappingHelper.LoadMappingsAsync(projectInfo.Root, projectInfo.Files);

            var filters = new List<string>();
            foreach (var filePath in selectedFiles)
            {
                var webResourceName = Path.GetFileName(filePath);
                var lowerFilePath = filePath.ToLower();

                if (webResourceName.ToLower() == MappingService.MappingFileName.ToLower())
                {
                    continue;
                }

                if (mappings != null && mappings.ContainsKey(lowerFilePath))
                {
                    webResourceName = mappings[lowerFilePath];
                }
                else if (projectConfig.IgnoreExtensions)
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

                if (webResourceName.ToLower() == MappingService.MappingFileName.ToLower())
                {
                    continue;
                }

                var relativePath = lowerFilePath.Replace(projectInfo.Root + "\\", "");

                if (mappings != null && mappings.ContainsKey(lowerFilePath))
                {
                    webResourceName = mappings[lowerFilePath];
                    await logger.WriteLineAsync("Mapping found: " + relativePath + " to " + webResourceName, projectConfig.ExtendedLog);
                }

                var webResource = webResources.FirstOrDefault(x => x.Name == webResourceName);
                if (webResource == null && projectConfig.IgnoreExtensions)
                {
                    await logger.WriteLineAsync(webResourceName + " does not exists in selected solution", projectConfig.ExtendedLog);
                    webResourceName = Path.GetFileNameWithoutExtension(filePath);
                    await logger.WriteLineAsync("Searching for " + webResourceName, projectConfig.ExtendedLog);
                    webResource = webResources.FirstOrDefault(x => x.Name == webResourceName);
                }
                if (webResource == null)
                {
                    await logger.WriteLineAsync("Uploading of " + webResourceName + " was skipped: web resource does not exists in selected solution", projectConfig.ExtendedLog);
                    await logger.WriteLineAsync(webResourceName + " does not exists in selected solution", !projectConfig.ExtendedLog);
                    continue;
                }
                if (!File.Exists(lowerFilePath))
                {
                    await logger.WriteLineAsync("Warning: File not found: " + lowerFilePath);
                    continue;
                }
                var isUpdated = await UpdateWebResourceByFileAsync(webResource, filePath, relativePath);
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
        private async Task<bool> UpdateWebResourceByFileAsync(WebResource webResource, string filePath, string relativePath)
        {
            var projectConfig = await settingsService.GetProjectConfigAsync();

            var webResourceName = Path.GetFileName(filePath);
            await logger.WriteLineAsync("Uploading " + webResourceName, projectConfig.ExtendedLog);

            //var projectRootPath = await projectHelper.GetSelectedProjectRootAsync();

            var localContent = GetEncodedFileContent(filePath);
            var remoteContent = webResource.Content;
            if (remoteContent.Length != localContent.Length || remoteContent != localContent)
            {
                await UpdateWebResourceByContentAsync(webResource, localContent);
                //var relativePath = filePath.Replace(projectRootPath + "\\", "");
                await logger.WriteLineAsync(webResource.Name + " uploaded from " + relativePath, !projectConfig.ExtendedLog);
                return true;
            }
            else
            {
                await logger.WriteLineAsync("Uploading of " + webResourceName + " was skipped: there aren't any change in the web resource", projectConfig.ExtendedLog);
                await logger.WriteLineAsync(webResourceName + " has no changes", !projectConfig.ExtendedLog);
                return false;
            }
        }

        /// <summary>
        /// Uploads web resource
        /// </summary>
        /// <param name="webResource">Web resource to be updated</param>
        /// <param name="content">Content to be set for web resource</param>
        private async Task UpdateWebResourceByContentAsync(WebResource webResource, string content)
        {
            var projectConfig = await settingsService.GetProjectConfigAsync();

            var name = webResource.Name;
            webResource.Content = content;
            var selectedEnvironment = projectConfig.GetSelectedEnvironment();
            await crmWebResourcesUpdaterClient.UploadWebresourceAsync(selectedEnvironment.ConnectionString.BuildConnectionString(), webResource);

            await logger.WriteLineAsync(name + " was successfully uploaded", projectConfig.ExtendedLog);
        }

        /// <summary>
        /// Retrieves web resources for selected items
        /// </summary>
        /// <returns>List of web resources</returns>
        private async Task<IEnumerable<WebResource>> RetrieveWebResourcesAsync(List<string> webResourceNames = null)
        {
            var projectConfig = await settingsService.GetProjectConfigAsync();

            await logger.WriteLineAsync("Retrieving existing web resources", projectConfig.ExtendedLog);
            var selectedEnvironment = projectConfig.GetSelectedEnvironment();
            var retrieveWebResourceResponse = await crmWebResourcesUpdaterClient.RetrieveWebResourcesAsync(selectedEnvironment.ConnectionString.BuildConnectionString(), selectedEnvironment.SelectedSolutionId, webResourceNames);
            if (retrieveWebResourceResponse.IsSuccessful == false)
            {
                throw new Exception($"Failed to retrieve web resource: {retrieveWebResourceResponse.Error}");
            }
            return retrieveWebResourceResponse.Payload;
        }

        /// <summary>
        /// Publishes webresources changes
        /// </summary>
        /// <param name="webresourcesIds">List of webresource IDs to publish</param>
        private async Task PublishWebResourcesAsync(IEnumerable<Guid> webresourcesIds)
        {
            await logger.WriteLineWithTimeAsync("Publishing...");

            if (webresourcesIds == null || !webresourcesIds.Any())
            {
                throw new ArgumentNullException("webresourcesId");
            }
            if (webresourcesIds.Any())
            {
                var projectConfig = await settingsService.GetProjectConfigAsync();
                var selectedEnvironment = projectConfig.GetSelectedEnvironment();
                await crmWebResourcesUpdaterClient.PublishWebResourcesAsync(selectedEnvironment.ConnectionString.BuildConnectionString(), webresourcesIds);
            }
            var count = webresourcesIds.Count();
            await logger.WriteLineWithTimeAsync(count + " file" + (count == 1 ? " was" : "s were") + " published");
        }

        private async Task CreateWebResourceAsync(EnvironmentConfig connectionInfo, WebResource webResource, string solution)
        {
            if (webResource == null)
            {
                throw new ArgumentNullException("Web resource can not be null");
            }
            await crmWebResourcesUpdaterClient.CreateWebresourceAsync(connectionInfo.ConnectionString.BuildConnectionString(), webResource, solution);
        }

        public async Task CreateWebResourceAsync()
        {
            var projectConfig = await settingsService.GetProjectConfigAsync();
            var selectedEnvironmentConfig = projectConfig.GetSelectedEnvironment();
            var selectedSolution = await solutionsService.GetSolutionDetailsAsync(selectedEnvironmentConfig);
            if (selectedSolution == null)
            {
                await logger.WriteLineAsync("Solution is not selected. Please check parameters");
                return;
            }
            await OpenCreateWebResourceFormAsync();
        }

        private async Task OpenCreateWebResourceFormAsync()
        {
            var projectConfig = await settingsService.GetProjectConfigAsync();
            var selectedEnvironmentConfig = projectConfig.GetSelectedEnvironment();
            if (selectedEnvironmentConfig == null)
            {
                await logger.WriteLineAsync("Environment (connection) is not selected or not found");
            }
            var selectedSolution = await solutionsService.GetSolutionDetailsAsync(selectedEnvironmentConfig);
            if (selectedSolution == null)
            {
                await logger.WriteLineAsync("Solution is not selected or not found");
            }

            var projectInfo = await vsDteHelper.GetSelectedProjectInfoAsync();
            var dialog = new CreateWebResourceForm(logger, projectInfo.SelectedFile, selectedSolution.PublisherPrefix);

            dialog.OnCreate = async (WebResource webResource) =>
            {
                webResource.Content = GetEncodedFileContent(projectInfo.SelectedFile);

                var webresourceName = webResource.Name;

                var isWebResourceExistsResponse = await crmWebResourcesUpdaterClient.IsWebResourceExistsAsync(selectedEnvironmentConfig.ConnectionString.BuildConnectionString(), webresourceName);
                if (isWebResourceExistsResponse.IsSuccessful == false)
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
                    var isMappingRequired = await mappingHelper.IsMappingRequiredAsync(
                        projectInfo.Root,
                        projectInfo.SelectedFiles,
                        projectInfo.SelectedFile,
                        webresourceName);

                    var isMappingFileReadOnly = await mappingHelper.IsMappingFileReadOnlyAsync(projectInfo.SelectedFiles);
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
                        await mappingHelper.CreateMappingAsync(
                            projectInfo.Guid,
                            projectInfo.Root,
                            projectInfo.SelectedFiles,
                            projectInfo.SelectedFile,
                            webresourceName);
                    }

                    await this.CreateWebResourceAsync(selectedEnvironmentConfig, webResource, selectedSolution.UniqueName);
                    await logger.WriteLineAsync("Webresource '" + webresourceName + "' was successfully created");
                    dialog.Close();
                }
                catch (Exception ex)
                {
                    Cursor.Current = Cursors.Arrow;
                    MessageBox.Show("An error occured during web resource creation: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    await logger.WriteLineAsync($"An error occured during web resource creation: {ex}", true);
                }
            };
            dialog.ShowDialog();
        }

        public string GetEncodedFileContent(String filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] binaryData = new byte[fs.Length];
            long bytesRead = fs.Read(binaryData, 0, (int)fs.Length);
            fs.Close();
            return System.Convert.ToBase64String(binaryData, 0, binaryData.Length);
        }
    }
}

