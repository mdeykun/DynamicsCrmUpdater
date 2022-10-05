using System;
using System.IO;
using System.Text;

namespace Cwru.Publisher.Helpers
{
    public static class FilesHelper
    {
        public static string GetEncodedFileContent(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] binaryData = new byte[fs.Length];
                fs.Read(binaryData, 0, (int)fs.Length);
                fs.Close();

                return Convert.ToBase64String(binaryData, 0, binaryData.Length);
            }
        }

        public static string GetFileContent(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var binaryData = new byte[fs.Length];
                fs.Read(binaryData, 0, (int)fs.Length);
                fs.Close();

                return Encoding.UTF8.GetString(binaryData);
            }
        }
    }
}
