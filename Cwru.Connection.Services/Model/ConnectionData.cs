using Cwru.Common.Config;
using Cwru.Common.Model;

namespace Cwru.Connection.Services.Model
{
    public class ConnectionData
    {
        public bool IsJustConfigured { get; set; }
        public bool IsValid { get; set; }
        public ProjectConfig ProjectConfig { get; set; }
        public ProjectInfo ProjectInfo { get; set; }
        public string Message { get; set; }
    }
}
