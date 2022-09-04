using Cwru.Common.Config;
using Cwru.Common.Model;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
using Task = System.Threading.Tasks.Task;

namespace Cwru.Common.Services
{
    public class VsDteService
    {
        private readonly IAsyncServiceProvider serviceProvider;
        private readonly Logger logger;

        public VsDteService(AsyncPackage asyncPackage, Logger logger)
        {
            this.serviceProvider = asyncPackage;
            this.logger = logger;
        }

        public async Task<ProjectInfo> GetSelectedProjectInfoAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var project = await GetSelectedProjectAsync();

            try
            {
                return new ProjectInfo()
                {
                    Root = Path.GetDirectoryName(project.FullName).ToLower(),
                    Guid = await GetProjectGuidAsync(project),
                    Files = await ExtractProjectFilesAsync(ToEnumerable(project.ProjectItems)),
                    SelectedFiles = await GetSelectedFilesAsync()
                };
            }
            catch (Exception ex)
            {
                await logger.WriteLineAsync(JsonConvert.SerializeObject(project));
                throw;
            }
        }

        public OleMenuCommand GetMenuCommand(Guid comandSet, int commandID, EventHandler invokeHandler)
        {
            CommandID menuCommandID = new CommandID(comandSet, commandID);
            return new OleMenuCommand(invokeHandler, menuCommandID);
        }

        public async Task SetStatusBarAsync(string message, object icon = null)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var svsStatusbar = await serviceProvider.GetServiceAsync(typeof(SVsStatusbar));
            if (svsStatusbar == null)
            {
                await logger.WriteLineAsync("Failed to access status bar");
                return;
            }
            var statusBar = svsStatusbar as IVsStatusbar;
            int frozen;
            statusBar.IsFrozen(out frozen);
            if (frozen == 0)
            {
                if (icon != null)
                {
                    statusBar.Animation(1, ref icon);
                }
                statusBar.SetText(message);
            }
        }

        public async Task SaveAllAsync()
        {
            var dte = await GetDteAsync();
            dte.ExecuteCommand("File.SaveAll");
        }

        private async Task<Project> GetSelectedProjectAsync()
        {
            var dte = await GetDteAsync();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            //UIHierarchyItem uiHierarchyItem = Enumerable.FirstOrDefault<UIHierarchyItem>(Enumerable.OfType<UIHierarchyItem>((IEnumerable)(object[])dte.ToolWindows.SolutionExplorer.SelectedItems));

            var selectedItems = (object[])dte.ToolWindows.SolutionExplorer.SelectedItems;
            var uiHierarchyItem = Enumerable.FirstOrDefault<UIHierarchyItem>(Enumerable.OfType<UIHierarchyItem>((IEnumerable)selectedItems));

            //Microsoft.VisualStudio.PlatformUI.UIHierarchyMarshaler


            var project = uiHierarchyItem.Object as Project;
            if (project != null)
            {
                return project;
            }
            var item = uiHierarchyItem.Object as ProjectItem;
            if (item != null)
            {
                return item.ContainingProject;
            }

            Document doc = dte.ActiveDocument;
            if (doc != null && doc.ProjectItem != null)
            {
                return doc.ProjectItem.ContainingProject;
            }
            return null;
        }

        private async Task<IEnumerable<ProjectItem>> GetSelectedItemsAsync()
        {
            var dte = await GetDteAsync();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var uiHierarchyItems = Enumerable.OfType<UIHierarchyItem>((IEnumerable)(object[])dte.ToolWindows.SolutionExplorer.SelectedItems);
            var items = new List<ProjectItem>();
            foreach (var uiItem in uiHierarchyItems)
            {
                var item = uiItem.Object as ProjectItem;
                if (item != null)
                {
                    items.Add(item);
                }
            }
            if (items.Count == 0)
            {
                Document doc = dte.ActiveDocument;
                if (doc != null && doc.ProjectItem != null)
                {
                    items.Add(doc.ProjectItem);
                }
            }
            return items;
        }

        private async Task<IEnumerable<string>> ExtractProjectFilesAsync(IEnumerable<ProjectItem> list)
        {
            if (list == null)
            {
                return null;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var files = new List<string>();
            foreach (ProjectItem item in list)
            {
                if (item.Kind.ToLower() == Consts.FileKindGuid.ToLower())
                {
                    var path = Path.GetDirectoryName(item.FileNames[0]).ToLower();
                    var fileName = Path.GetFileName(item.FileNames[0]);
                    files.Add(path + "\\" + fileName);
                }

                if (item.ProjectItems != null)
                {
                    var childItems = await ExtractProjectFilesAsync(ToEnumerable(item.ProjectItems));
                    files.AddRange(childItems);
                }
            }

            return files;
        }

        public async Task AddFromFileAsync(Guid projectGuid, string filePath)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var project = await GetProjectByGuidAsync(projectGuid);
            if (project == null)
            {
                throw new InvalidOperationException("Failed to load project by guid");
            }

            project.ProjectItems.AddFromFile(filePath);
        }

        private async Task<IEnumerable<string>> GetSelectedFilesAsync()
        {
            var selectedItems = await GetSelectedItemsAsync();
            return await ExtractProjectFilesAsync(selectedItems);
        }

        private async Task<Project> GetProjectByGuidAsync(Guid projectGuid)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var svsSolution = await serviceProvider.GetServiceAsync(typeof(SVsSolution));
            var solution = svsSolution as IVsSolution;
            if (solution == null)
            {
                return null;
            }

            var result = solution.GetProjectOfGuid(projectGuid, out IVsHierarchy hierarchy);
            if (!ErrorHandler.Succeeded(result))
            {
                return null;
            }

            result = hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out object obj);
            if (!ErrorHandler.Succeeded(result))
            {
                return null;
            }

            var project = obj as Project;
            if (project == null)
            {
                return null;
            }

            return project;
        }

        private async Task<Guid> GetProjectGuidAsync(Project project)
        {
            Guid projectGuid = Guid.Empty;
            IVsHierarchy hierarchy;
            var svsSolution = await serviceProvider.GetServiceAsync(typeof(SVsSolution));
            if (svsSolution == null)
            {
                throw new InvalidOperationException("Failed to load project guid");
            }
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var solution = svsSolution as IVsSolution;
            solution.GetProjectOfUniqueName(project.FullName, out hierarchy);
            if (hierarchy != null)
            {
                solution.GetGuidOfProject(hierarchy, out projectGuid);
            }
            return projectGuid;
        }

        private static IEnumerable<ProjectItem> ToEnumerable(ProjectItems projectItems)
        {
            var list = new List<ProjectItem>();
            foreach (ProjectItem item in projectItems)
            {
                list.Add(item);
            }

            return list;
        }

        private async Task<EnvDTE80.DTE2> GetDteAsync()
        {
            var dte = await serviceProvider.GetServiceAsync(typeof(DTE)) as EnvDTE80.DTE2;
            if (dte == null)
            {
                await logger.WriteLineAsync("Failed to get DTE service.");
                throw new Exception("Failed to get DTE service.");
            }

            return dte;
        }

        //public async Task<string> GetProjectRootAsync(Project project)
        //{
        //    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        //    return Path.GetDirectoryName(project.FullName).ToLower();
        //}

        //private async Task<string> GetSelectedFilePathAsync()
        //{
        //    var files = await GetSelectedFilesAsync();
        //    return files.FirstOrDefault();
        //}

        //public async Task<IEnumerable<string>> GetProjectFilesAsync()
        //{
        //    var selectedProject = await GetSelectedProjectAsync();
        //    if (selectedProject == null)
        //    {
        //        return null;
        //    }
        //    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        //    var projectFiles = await GetProjectFilesAsync(selectedProject.ProjectItems);
        //    return projectFiles;
        //}
    }
}
