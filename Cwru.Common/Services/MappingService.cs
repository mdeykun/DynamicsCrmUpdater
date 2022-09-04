using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Task = System.Threading.Tasks.Task;

namespace Cwru.Common.Services
{
    public class MappingService
    {
        private readonly VsDteService vsDteHelper;
        private readonly ConfigurationService settingsService;

        public const string MappingFileName = "UploaderMapping.config";

        public MappingService(VsDteService vsDteHelper, ConfigurationService settingsService)
        {
            this.vsDteHelper = vsDteHelper;
            this.settingsService = settingsService;
        }

        public async Task<Dictionary<string, string>> LoadMappingsAsync(string projectRootPath, IEnumerable<string> projectFiles)
        {
            var mappingFilePath = await this.GetMappingFilePathAsync(projectFiles);
            if (mappingFilePath == null)
            {
                return null;
            }

            var mappingList = new Dictionary<string, string>();

            XDocument doc = XDocument.Load(mappingFilePath);
            var mappings = doc.Descendants("Mapping");
            var projectFilesToProcess = projectFiles.ToList();
            foreach (var mapping in mappings)
            {
                var shortScriptPath = mapping.Attribute("localPath") == null ? null : mapping.Attribute("localPath").Value;
                shortScriptPath = shortScriptPath ?? (mapping.Attribute("scriptPath") == null ? null : mapping.Attribute("scriptPath").Value);
                if (shortScriptPath == null)
                {
                    throw new ArgumentNullException("Mappings contains 'Mapping' tag without 'localPath' attribute");
                }
                var scriptPath = projectRootPath + "\\" + shortScriptPath;
                scriptPath = scriptPath.ToLower();
                var webResourceName = mapping.Attribute("webResourceName") == null ? null : mapping.Attribute("webResourceName").Value;
                if (webResourceName == null)
                {
                    throw new ArgumentNullException("Mappings contains 'Mapping' tag without 'webResourceName' attribute");
                }
                if (mappingList.ContainsKey(scriptPath))
                {
                    throw new ArgumentException("Mappings contains dublicate for \"" + shortScriptPath + "\"");
                }
                projectFilesToProcess.RemoveAll(x => x.ToLower() == scriptPath);
                mappingList.Add(scriptPath, webResourceName);
            }
            foreach (var mapping in mappingList)
            {
                var scriptPath = mapping.Key;
                var webResourceName = mapping.Value;
                var projectMappingDublicates = projectFilesToProcess.Where(x => Path.GetFileName(x).ToLower() == webResourceName.ToLower());
                if (projectMappingDublicates.Count() > 0)
                {
                    throw new ArgumentException("Project contains dublicate(s) for mapped web resource \"" + webResourceName + "\"");
                }

            }
            return mappingList;
        }

        public async Task CreateMappingAsync(Guid projectId, string projectRoot, IEnumerable<string> projectFiles, string filePath, string webresourceName)
        {
            var mappingFilePath = await this.GetMappingFilePathAsync(projectFiles);
            if (mappingFilePath == null)
            {
                mappingFilePath = await this.CreateMappingFileAsync(projectId, projectRoot, projectFiles);
            }

            //var projectRoot = await projectHelper.GetProjectRootAsync(project);
            var projectRootPath = projectRoot + "\\";
            var scriptShortPath = filePath.Replace(projectRootPath, "");

            XDocument doc = XDocument.Load(mappingFilePath);
            var mapping = new XElement("Mapping");
            mapping.SetAttributeValue("localPath", scriptShortPath);
            mapping.SetAttributeValue("webResourceName", webresourceName);
            doc.Element("Mappings").Add(mapping);
            doc.Save(mappingFilePath);
        }

        public async Task<bool> IsMappingFileReadOnlyAsync(IEnumerable<string> projectFiles)
        {
            var path = await GetMappingFilePathAsync(projectFiles);
            if (path == null)
            {
                return false;
            }
            var fileInfo = new FileInfo(path);
            return fileInfo.IsReadOnly;
        }

        public async Task<bool> IsMappingRequiredAsync(string projectRoot, IEnumerable<string> projectFiles, string projectItemPath, string webresourceName)
        {
            var fileName = Path.GetFileName(projectItemPath);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(projectItemPath);
            if (fileName == webresourceName)
            {
                return false;
            }
            var settings = await settingsService.GetProjectConfigAsync();
            var uploadWithoutExtension = settings.IgnoreExtensions;
            if (fileNameWithoutExtension == webresourceName)
            {
                return false;
            }
            if (await MappingAlreadyExistsAsync(projectRoot, projectFiles, webresourceName))
            {
                return false;
            }
            return true;
        }

        private async Task<bool> MappingAlreadyExistsAsync(string projectRoot, IEnumerable<string> projectFiles, string webresourceName)
        {
            var mappings = await LoadMappingsAsync(projectRoot, projectFiles);
            return mappings.Any(x => x.Value == webresourceName);
        }

        public async Task<string> CreateMappingFileAsync(Guid projectGuid, string projectRoot, IEnumerable<string> projectFiles)
        {
            var filePath = projectRoot + "\\UploaderMapping.config";

            if (File.Exists(filePath))
            {
                var path = await GetMappingFilePathAsync(projectFiles);
                if (path == null)
                {
                    await vsDteHelper.AddFromFileAsync(projectGuid, filePath);
                }
                return filePath;
            }

            using (var writer = File.CreateText(projectRoot + "\\UploaderMapping.config"))
            {
                var mappingFileLines = new string[]
                {
                    "<?xml version=\"1.0\" encoding=\"utf-8\" ?>",
                    "<Mappings  xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"	xsi:noNamespaceSchemaLocation=\"http://exitoconsulting.ru/schema/CrmWebResourcesUpdater/MappingSchema.xsd\">",
                    "    <!--",
                    "    EXAMPLES OF MAPPINGS:",
                    "    <Mapping localPath=\"scripts\\contact.js\" webResourceName=\"new_contact\"/>",
                    "    <Mapping localPath=\"account.js\" webResourceName=\"new_account\"/>",
                    "    -->",
                    "</Mappings>",
                };

                var mappingFileContent = string.Join(Environment.NewLine, mappingFileLines);
                await writer.WriteAsync(mappingFileContent);
            }

            await vsDteHelper.AddFromFileAsync(projectGuid, filePath);
            return filePath;
        }

        private async Task<string> GetMappingFilePathAsync(IEnumerable<string> projectFiles)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (projectFiles == null || projectFiles.Count() == 0)
            {
                return null;
            }

            foreach (var file in projectFiles)
            {
                if (Path.GetFileName(file).ToLower() == MappingFileName.ToLower())
                {
                    return file.ToLower();
                }
            }

            return null;
        }
    }
}
