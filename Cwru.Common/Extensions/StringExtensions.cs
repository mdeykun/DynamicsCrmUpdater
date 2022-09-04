using System.Security;

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
    }
}
