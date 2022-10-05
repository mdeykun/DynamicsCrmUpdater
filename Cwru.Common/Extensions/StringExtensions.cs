using System;
using System.IO;
using System.Linq;
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
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            if (string.IsNullOrEmpty(rootPath))
            {
                return path;
            }

            var result = path.ToLower().Replace(rootPath.ToLower(), "");
            return result.TrimStart('\\');
        }

        public static bool IsEqualToLower(this string val1, params string[] vals)
        {
            return vals.Any(x => string.Compare(val1, x, true) == 0);
        }

        public static bool EndWithLower(this string val1, string val2)
        {
            return val1.EndsWith(val2, StringComparison.OrdinalIgnoreCase);
        }

        public static bool StartWithLower(this string val1, string val2)
        {
            return val1.StartsWith(val2, StringComparison.OrdinalIgnoreCase);
        }

        public static string RemoveIllegalFileNameSymbols(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var regexSearch = new string(Path.GetInvalidFileNameChars());// + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(value, "");
        }
    }
}
