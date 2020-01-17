using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmWebResourcesUpdater.Common.Helpers
{
    public static class FileHelper
    {
        /// <summary>
        /// Reads encoded content from file
        /// </summary>
        /// <param name="filePath">Path to file to read content from</param>
        /// <returns>Returns encoded file contents as a System.String</returns>
        public static string GetEncodedFileContent(String filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] binaryData = new byte[fs.Length];
            long bytesRead = fs.Read(binaryData, 0, (int)fs.Length);
            fs.Close();
            return System.Convert.ToBase64String(binaryData, 0, binaryData.Length);
        }
    }
}
