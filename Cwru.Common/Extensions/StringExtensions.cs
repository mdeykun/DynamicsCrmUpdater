using System;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;

namespace Cwru.Common.Extensions
{
    public static class StringExtensions
    {
        public static SecureString ToSecureString(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            var secureString = new SecureString();
            foreach (var c in value)
            {
                secureString.AppendChar(c);
            }

            return secureString;
        }

        public static string AddRoot(this string path, string rootPath)
        {
            return Path.Combine(rootPath, path);
        }

        public static string RemoveRoot(this string path, string rootPath)
        {
            var result = Regex.Replace(path, rootPath, "", RegexOptions.IgnoreCase);
            return result.TrimStart('\\');
        }

        public static bool IsEqualToLower(this string val1, string val2)
        {
            return string.Compare(val1, val2, true) == 0;
        }

        public static bool EndWithLower(this string val1, string val2)
        {
            return val1.EndsWith(val2, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
