using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Cwru.Common.Extensions
{
    public static class SecureStringExtensions
    {
        public static string GetString(this SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
    }
}
