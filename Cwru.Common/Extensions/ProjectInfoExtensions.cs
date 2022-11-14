using Cwru.Common.Model;
using System.Collections.Generic;
using System.Linq;

namespace Cwru.Common.Extensions
{
    public static class ProjectInfoExtensions
    {
        public static IEnumerable<string> GetFilesPaths(this ProjectInfo projectInfo)
        {
            return projectInfo.ElementsFlat.Where(x => x.Type == SolutionElementType.File).Select(x => x.FilePath).ToList();
        }

        public static IEnumerable<string> GetFilesPathsUnder(this ProjectInfo projectInfo, string selectedFolderPath)
        {
            var filePaths = projectInfo.GetFilesPaths();
            return filePaths.Where(x => x.StartWithLower(selectedFolderPath)).ToList();
        }

        public static string GetSelectedFolderPath(this ProjectInfo projectInfo)
        {
            return GetElementsFlatList(projectInfo.SelectedElements).FirstOrDefault(x => x.Type == SolutionElementType.Folder)?.FilePath;
        }

        public static IEnumerable<string> GetFilesInSelectedFolder(this ProjectInfo projectInfo, string folderPath)
        {
            return projectInfo.ElementsFlat.Where(x => x.Type == SolutionElementType.File && x.FilePath.StartWithLower(folderPath)).Select(x => x.FilePath).ToList();
        }

        public static IEnumerable<string> GetSelectedFilesPaths(this ProjectInfo projectInfo)
        {
            return projectInfo.SelectedElements.Where(x => x.Type == SolutionElementType.File).Select(x => x.FilePath).ToList();
        }

        public static string GetSelectedFilePath(this ProjectInfo projectInfo)
        {
            return projectInfo.SelectedElements.FirstOrDefault(x => x.Type == SolutionElementType.File)?.FilePath;
        }

        public static bool ContainsFile(this ProjectInfo projectInfo, string filePath)
        {
            return projectInfo.ElementsFlat.Any(x => filePath.IsEqualToLower(x.FilePath));
        }

        public static SolutionElement GetElement(this ProjectInfo projectInfo, string filePath)
        {
            return projectInfo.ElementsFlat.FirstOrDefault(x => filePath.IsEqualToLower(x.FilePath));
        }

        private static IEnumerable<SolutionElement> GetElementsFlatList(IEnumerable<SolutionElement> solutionElements)
        {
            var result = new List<SolutionElement>();
            if (solutionElements == null || !solutionElements.Any())
            {
                return result;
            }

            foreach (var element in solutionElements)
            {
                result.Add(element);
                result.AddRange(GetElementsFlatList(element.Childs));
            }

            return result;
        }
    }
}
