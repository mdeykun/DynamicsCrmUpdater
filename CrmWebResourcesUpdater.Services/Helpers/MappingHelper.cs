using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CrmWebResourcesUpdater.Services.Helpers
{
    public class MappingHelper
    {
        ProjectHelper projectHelper;
        public MappingHelper(AsyncPackage asyncPackage)
        {
            this.projectHelper = new ProjectHelper(asyncPackage);
        }
        public Dictionary<string, string> LoadMappings(Project project)
        {
            var projectRootPath = projectHelper.GetProjectRoot(project);
            var projectFiles = projectHelper.GetProjectFiles(project);
            var mappingFilePath = this.GetMappingFilePath(project);
            if (mappingFilePath == null)
            {
                return null;
            }

            var mappingList = new Dictionary<string, string>();

            XDocument doc = XDocument.Load(mappingFilePath);
            var mappings = doc.Descendants("Mapping");
            foreach (var mapping in mappings)
            {
                var shortScriptPath = mapping.Attribute("localPath") == null ? null : mapping.Attribute("localPath").Value;
                shortScriptPath = shortScriptPath ?? (mapping.Attribute("scriptPath") == null ? null : mapping.Attribute("scriptPath").Value);
                if(shortScriptPath == null)
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
                projectFiles.RemoveAll(x => x.ToLower() == scriptPath);
                mappingList.Add(scriptPath, webResourceName);
            }
            foreach(var mapping in mappingList)
            {
                var scriptPath = mapping.Key;
                var webResourceName = mapping.Value;
                var projectMappingDublicates = projectFiles.Where(x => Path.GetFileName(x).ToLower() == webResourceName.ToLower());
                if (projectMappingDublicates.Count() > 0)
                {
                    throw new ArgumentException("Project contains dublicate(s) for mapped web resource \"" + webResourceName + "\"");
                }

            }
            return mappingList;
        }

        public void CreateMapping(Project project, string filePath, string webresourceName)
        {
            var mappingFilePath = this.GetMappingFilePath(project);
            if (mappingFilePath == null)
            {
                mappingFilePath = this.CreateMappingFile(project);
            }

            var projectRootPath = projectHelper.GetProjectRoot(project) + "\\";
            var scriptShortPath = filePath.Replace(projectRootPath, "");

            XDocument doc = XDocument.Load(mappingFilePath);
            var mapping = new XElement("Mapping");
            mapping.SetAttributeValue("localPath", scriptShortPath);
            mapping.SetAttributeValue("webResourceName", webresourceName);
            doc.Element("Mappings").Add(mapping);
            doc.Save(mappingFilePath);
        }

        public bool IsMappingFileReadOnly(Project project)
        {
            var path = GetMappingFilePath(project);
            if(path == null)
            {
                return false;
            }
            var fileInfo = new FileInfo(path);
            return fileInfo.IsReadOnly;
        }

        public async Task<bool> IsMappingRequiredAsync(Project project, string projectItemPath, string webresourceName)
        {
            var fileName = Path.GetFileName(projectItemPath);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(projectItemPath);
            if (fileName == webresourceName)
            {
                return false;
            }
            var settings = await SettingsService.Instance.GetSettingsAsync();
            var uploadWithoutExtension = settings.CrmConnections.IgnoreExtensions;
            if(fileNameWithoutExtension == webresourceName)
            {
                return false;
            }
            return true;
        }

        public string CreateMappingFile(Project project)
        {
            var projectPath = projectHelper.GetProjectRoot(project);
            var filePath = projectPath + "\\UploaderMapping.config";
            if (File.Exists(filePath))
            {
                var path = GetMappingFilePath(project);
                if (path == null)
                {
                    project.ProjectItems.AddFromFile(filePath);
                }
                return filePath;
            }
            var writer = File.CreateText(projectPath + "\\UploaderMapping.config");
            writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            writer.WriteLine("<Mappings  xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"	xsi:noNamespaceSchemaLocation=\"http://exitoconsulting.ru/schema/CrmWebResourcesUpdater/MappingSchema.xsd\">");
            writer.WriteLine("<!--");
            writer.WriteLine("EXAMPLES OF MAPPINGS:");
            writer.WriteLine("<Mapping localPath=\"scripts\\contact.js\" webResourceName=\"new_contact\"/>");
            writer.WriteLine("<Mapping localPath=\"account.js\" webResourceName=\"new_account\"/>");
            writer.WriteLine("-->");
            writer.WriteLine("</Mappings>");
            writer.Flush();
            writer.Close();
            project.ProjectItems.AddFromFile(filePath);
            return filePath;
        }

        public string GetMappingFilePath(Project project)
        {
            var projectFiles = projectHelper.GetProjectFiles(project.ProjectItems);
            if (projectFiles == null || projectFiles.Count == 0)
            {
                return null;
            }
            foreach (var file in projectFiles)
            {
                if (Path.GetFileName(file).ToLower() == Settings.MappingFileName.ToLower())
                {
                    return file.ToLower();
                }
            }
            return null;
        }
    }
}
