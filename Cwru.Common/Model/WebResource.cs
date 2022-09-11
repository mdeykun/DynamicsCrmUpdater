using System;

namespace Cwru.Common.Model
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
