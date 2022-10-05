using Cwru.Common.Extensions;
using Cwru.Common.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        public Dictionary<string, string> LoadMappings(ProjectInfo projectInfo)
        {
            var mappingList = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            var mappingFilePath = MappingFileName.AddRoot(projectInfo.Root);
            if (!projectInfo.ContainsFile(mappingFilePath))
            {
                return mappingList;
            }

            var doc = XDocument.Load(mappingFilePath);
            var mappings = doc.Descendants("Mapping");
            var projectFilesToProcess = projectInfo.GetFilesPaths().ToList();

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

                var scriptPath = shortScriptPath.AddRoot(projectInfo.Root);
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

        public async Task CreateMappingAsync(ProjectInfo projectInfo, string filePath, string webResourceName)
        {
            var isMappingFileReadOnly = IsMappingFileReadOnly(projectInfo);
            if (isMappingFileReadOnly)
            {
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

            if (!isMappingFileReadOnly)
            {
                await AddMappingAsync(projectInfo, filePath, webResourceName);
            }
        }

        public async Task CreateMappingFileAsync(ProjectInfo projectInfo)
        {
            var mappingFilePath = MappingFileName.AddRoot(projectInfo.Root);

            if (!File.Exists(mappingFilePath))
            {
                using (var writer = File.CreateText(mappingFilePath))
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
            }

            if (!projectInfo.ContainsFile(mappingFilePath))
            {
                await vsDteService.AddFromFileAsync(projectInfo.Guid, mappingFilePath);
            }
        }

        public bool IsMappingRequired(Dictionary<string, string> mappings, string filePath, string webResourceName)
        {
            var fileName = Path.GetFileName(filePath);
            if (fileName.IsEqualToLower(webResourceName))
            {
                return false;
            }

            if (mappings.Any(x => x.Key.IsEqualToLower(filePath) && x.Value.IsEqualToLower(webResourceName)))
            {
                return false;
            }

            return true;
        }

        public string GetMappingByFilePath(Dictionary<string, string> mappings, string filePath)
        {
            return mappings.Where(x => x.Key.IsEqualToLower(filePath)).Select(x => x.Value).FirstOrDefault();
        }

        private async Task AddMappingAsync(ProjectInfo projectInfo, string filePath, string webresourceName)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(filePath.RemoveRoot(projectInfo.Root)))
            {
                throw new ArgumentNullException(nameof(filePath), "Mapping requres non empty file path");
            }

            if (string.IsNullOrEmpty(webresourceName))
            {
                throw new ArgumentNullException(nameof(webresourceName), "Mapping requres non empty webresource name");
            }

            var mappingFilePath = MappingFileName.AddRoot(projectInfo.Root);
            if (!projectInfo.ContainsFile(mappingFilePath))
            {
                await CreateMappingFileAsync(projectInfo);
            }
            else
            {
                var mappings = LoadMappings(projectInfo);
                if (MappingExists(mappings, filePath, webresourceName))
                {
                    return;
                }
            }

            var doc = XDocument.Load(mappingFilePath);

            doc.Element("Mappings").Add(
                new XElement("Mapping",
                    new XAttribute("localPath", filePath.RemoveRoot(projectInfo.Root)),
                    new XAttribute("webResourceName", webresourceName)));

            doc.Save(mappingFilePath);
        }

        private bool IsMappingFileReadOnly(ProjectInfo projectInfo)
        {
            var mappingFilePath = MappingFileName.AddRoot(projectInfo.Root);
            return projectInfo.ContainsFile(mappingFilePath) ? new FileInfo(mappingFilePath).IsReadOnly : false;
        }

        private bool MappingExists(Dictionary<string, string> mappings, string filePath, string webResourceName)
        {
            return mappings.Any(x => x.Key.IsEqualToLower(filePath) && x.Value.IsEqualToLower(webResourceName));
        }
    }
}
