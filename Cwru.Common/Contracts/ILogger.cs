using System;
using System.Threading.Tasks;

namespace Cwru.Common
{
    public interface ILogger
    {
        Task ClearAsync();
        Task WriteAsync(Exception ex);
        Task WriteAsync(string message);
        Task WriteAsync(string message, Exception ex);
        Task WriteLineAsync(string message, bool print = true);
        Task WriteLineWithTimeAsync(string message, bool print = true);
    }
}