using Cwru.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cwru.Common.Services
{
    public class MappingService
    {
        private readonly VsDteService vsDteService;
        public const string MappingFileName = "UploaderMapping.config";

        public MappingService(VsDteService vsDteHelper)
        {
            this.vsDteService = vsDteHelper;
        }

        public Dictionary<string, string> LoadMappings(string projectRootPath, IEnumerable<string> projectFiles)
        {
            var mappingList = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            var mappingFilePath = GetMappingFilePath(projectFiles);
            if (mappingFilePath == null)
            {
                return mappingList;
            }

            var doc = XDocument.Load(mappingFilePath);
            var mappings = doc.Descendants("Mapping");
            var projectFilesToProcess = projectFiles.ToList();

            foreach (var mapping in mappings)
            {
                var shortScriptPath = mapping.Attribute("localPath")?.Value ?? mapping.Attribute("scriptPath")?.Value;
                if (shortScriptPath == null)
                {
                    throw new ArgumentNullException("Mappings contains 'Mapping' tag without 'localPath' attribute");
                }
                var webResourceName = mapping.Attribute("webResourceName")?.Value;
                if (webResourceName == null)
                {
                    throw new ArgumentNullException("Mappings contains 'Mapping' tag without 'webResourceName' attribute");
                }

                var scriptPath = shortScriptPath.AddRoot(projectRootPath);
                if (mappingList.ContainsKey(scriptPath))
                {
                    throw new ArgumentException($"Mappings contains dublicate for \"{shortScriptPath}\"");
                }

                projectFilesToProcess.RemoveAll(x => x.IsEqualToLower(scriptPath));
                mappingList.Add(scriptPath, webResourceName);
            }

            foreach (var webResourceName in mappingList.Values)
            {
                var mappingDublicates = projectFilesToProcess.
                    Select(x => Path.GetFileName(x)).
                    Count(x => x.IsEqualToLower(webResourceName));

                if (mappingDublicates > 0)
                {
                    throw new ArgumentException($"Project contains dublicate(s) for mapped web resource \"{webResourceName}\"");
                }

            }
            return mappingList;
        }

        public async Task CreateMappingAsync(Guid projectId, string projectRoot, IEnumerable<string> projectFiles, string filePath, string webresourceName)
        {
            var mappingFilePath = GetMappingFilePath(projectFiles) ??
                await CreateMappingFileAsync(projectId, projectRoot, projectFiles);

            var doc = XDocument.Load(mappingFilePath);

            doc.Element("Mappings").Add(
                new XElement("Mapping",
                    new XAttribute("localPath", filePath.RemoveRoot(projectRoot)),
                    new XAttribute("webResourceName", webresourceName)));

            doc.Save(mappingFilePath);
        }

        public bool IsMappingFileReadOnly(IEnumerable<string> projectFiles)
        {
            var path = GetMappingFilePath(projectFiles);
            return path != null ? new FileInfo(path).IsReadOnly : false;
        }

        public bool IsMappingRequired(string projectRoot, IEnumerable<string> projectFiles, string projectItemPath, string webresourceName, bool ignoreExtension)
        {
            var fileName = Path.GetFileName(projectItemPath);
            if (fileName == webresourceName)
            {
                return false;
            }

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(projectItemPath);
            if (ignoreExtension && fileNameWithoutExtension == webresourceName)
            {
                return false;
            }

            var mappings = LoadMappings(projectRoot, projectFiles);
            if (mappings.Any(x => x.Value == webresourceName))
            {
                return false;
            }

            return true;
        }

        public async Task<string> CreateMappingFileAsync(Guid projectGuid, string projectRoot, IEnumerable<string> projectFiles)
        {
            var filePath = MappingFileName.AddRoot(projectRoot);

            if (File.Exists(filePath))
            {
                var path = GetMappingFilePath(projectFiles);
                if (path == null)
                {
                    await vsDteService.AddFromFileAsync(projectGuid, filePath);
                }
                return filePath;
            }

            using (var writer = File.CreateText(filePath))
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

            await vsDteService.AddFromFileAsync(projectGuid, filePath);
            return filePath;
        }

        private string GetMappingFilePath(IEnumerable<string> projectFiles)
        {
            return projectFiles != null ?
                projectFiles.FirstOrDefault(x => x.EndWithLower(MappingFileName)) :
                null;
        }
    }
}
