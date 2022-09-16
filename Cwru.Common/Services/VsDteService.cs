using Cwru.Common.Model;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
using Task = System.Threading.Tasks.Task;

namespace Cwru.Common.Services
{
    public class VsDteService
    {
        public const string FileKindGuid = "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";
        public const string ProjectKindGuid = "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";

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
            if (project == null)
            {
                return null;
            }

            var projectGuid = await GetProjectGuidAsync(project);
            if (projectGuid == Guid.Empty)
            {
                throw new Exception("Project guid can't be retrived");
            }

            var selectedFiles = await GetFilePathsAsync(await GetSelectedItemsAsync());
            return new ProjectInfo()
            {
                Root = Path.GetDirectoryName(project.FullName).ToLower(),
                Guid = projectGuid,
                Files = await GetFilePathsAsync(await GetProjectItemsAsync(project)),
                SelectedFiles = selectedFiles,
                SelectedFile = selectedFiles.FirstOrDefault()
            };
        }

        public async Task SetStatusBarAsync(string message, object icon = null)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var statusBar = await GetStatusBarServiceAsync();

            statusBar.IsFrozen(out var frozen);
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
            var dte = await GetDteServiceAsync();
            dte.ExecuteCommand("File.SaveAll");
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

        public async Task OpenFileAndPlaceContentAsync(Guid projectGuid, string file, string content)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var project = await GetProjectByGuidAsync(projectGuid);
            if (project == null)
            {
                throw new InvalidOperationException("Failed to load project by guid");
            }

            var projectItems = await GetProjectItemsAsync(project);
            var projectItem = projectItems.FirstOrDefault(x => string.Compare(x.FileNames[0].ToLower(), file, true) == 0);
            if (projectItem != null)
            {
                projectItem.Open(EnvDTE.Constants.vsViewKindCode);
                projectItem.Document.ActiveWindow.Activate();

                var textDocument = (TextDocument)projectItem.Document.Object("TextDocument");

                var startPoint = textDocument.CreateEditPoint(textDocument.StartPoint);
                startPoint.Delete(textDocument.EndPoint);
                startPoint.Insert(content);
            }
        }

        private async Task<Project> GetSelectedProjectAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var selectedItems = await GetSelectedHierarchyItemsAsync();
            var selectedItem = selectedItems.FirstOrDefault();

            var project = selectedItem?.Object as Project;
            if (project != null)
            {
                return project;
            }

            var item = selectedItem?.Object as ProjectItem;
            if (item != null)
            {
                return item.ContainingProject;
            }

            var dte = await GetDteServiceAsync();
            Document doc = dte.ActiveDocument;
            if (doc != null && doc.ProjectItem != null)
            {
                return doc.ProjectItem.ContainingProject;
            }
            return null;
        }

        private async Task<IEnumerable<ProjectItem>> GetSelectedItemsAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var selectedItems = await GetSelectedHierarchyItemsAsync();
            var projectItems = new List<ProjectItem>();

            foreach (var uiItem in selectedItems)
            {
                var item = uiItem.Object as ProjectItem;
                if (item != null)
                {
                    projectItems.Add(item);
                }
            }

            if (projectItems.Count == 0)
            {
                var dte = await GetDteServiceAsync();
                Document doc = dte.ActiveDocument;
                if (doc != null && doc.ProjectItem != null)
                {
                    projectItems.Add(doc.ProjectItem);
                }
            }

            return await GetProjectItemsAsync(projectItems);
        }

        private async Task<IEnumerable<ProjectItem>> GetProjectItemsAsync(Project project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (project == null)
            {
                return Enumerable.Empty<ProjectItem>();
            }

            return await GetProjectItemsAsync(ToEnumerable(project.ProjectItems));
        }

        private async Task<IEnumerable<ProjectItem>> GetProjectItemsAsync(IEnumerable<ProjectItem> itemsToCheck)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (itemsToCheck == null || itemsToCheck.Count() == 0)
            {
                return Enumerable.Empty<ProjectItem>();
            }

            var result = new List<ProjectItem>();

            foreach (var item in itemsToCheck)
            {
                if (item.Kind.ToLower() == FileKindGuid.ToLower())
                {
                    result.Add(item);
                }

                if (item.ProjectItems != null)
                {
                    var childItems = await GetProjectItemsAsync(ToEnumerable(item.ProjectItems));
                    result.AddRange(childItems);
                }
            }

            return result;
        }

        private async Task<Project> GetProjectByGuidAsync(Guid projectGuid)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var solution = await GetSolutionServiceAsync();
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
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var solution = await GetSolutionServiceAsync();
            var result = solution.GetProjectOfUniqueName(project.FullName, out IVsHierarchy hierarchy);
            if (!ErrorHandler.Succeeded(result) || hierarchy == null)
            {
                return Guid.Empty;
            }

            result = solution.GetGuidOfProject(hierarchy, out Guid projectGuid);
            if (!ErrorHandler.Succeeded(result))
            {
                return Guid.Empty;
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

        private async Task<IEnumerable<UIHierarchyItem>> GetSelectedHierarchyItemsAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = await GetDteServiceAsync();
            var selectedItems = (object[])dte.ToolWindows.SolutionExplorer.SelectedItems;
            var result = selectedItems != null ? selectedItems.AsEnumerable().OfType<UIHierarchyItem>() : Enumerable.Empty<UIHierarchyItem>();

            return result;
        }

        private async Task<IEnumerable<string>> GetFilePathsAsync(IEnumerable<ProjectItem> projectItems)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return projectItems.Select(x => x.FileNames[0].ToLower()).ToList();
            //var path = Path.GetDirectoryName(item.FileNames[0]).ToLower();
            //var fileName = Path.GetFileName(item.FileNames[0]);
            //files.Add(path + "\\" + fileName);

        }

        private EnvDTE80.DTE2 dte = null;
        private async Task<EnvDTE80.DTE2> GetDteServiceAsync()
        {
            if (dte != null)
            {
                return dte;
            }

            dte = await serviceProvider.GetServiceAsync(typeof(DTE)) as EnvDTE80.DTE2;
            if (dte == null)
            {
                await logger.WriteLineAsync("Failed to get DTE service.");
                throw new Exception("Failed to get DTE service.");
            }

            return dte;
        }

        private IVsStatusbar statusBar = null;
        private async Task<IVsStatusbar> GetStatusBarServiceAsync()
        {
            if (statusBar != null)
            {
                return statusBar;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            statusBar = await serviceProvider.GetServiceAsync(typeof(SVsStatusbar)) as IVsStatusbar;
            if (statusBar == null)
            {
                await logger.WriteLineAsync("Failed to access status bar service");

            }

            return statusBar;
        }

        private IVsSolution solution = null;
        private async Task<IVsSolution> GetSolutionServiceAsync()
        {
            if (solution != null)
            {
                return solution;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            solution = await serviceProvider.GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
            if (solution == null)
            {
                throw new InvalidOperationException("Failed to load solution service");
            }

            return solution;
        }
    }
}
