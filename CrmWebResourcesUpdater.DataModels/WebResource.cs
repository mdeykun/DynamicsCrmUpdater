using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmWebResourcesUpdater.DataModel
{
    public class WebResource
    {
        public string Name { get; set; }
        public Guid? Id { get; set; }
        public string Content { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
    }
}
