using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmWebResourcesUpdater.Service.Common
{
    public class UpdaterServiceResponse<T>
    {
        public bool IsSuccessful { get; set; }
        public string Error { get; set; }
        public T Payload { get; set; }
    }
}
