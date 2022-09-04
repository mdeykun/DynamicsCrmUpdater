using System;
using System.Collections.Generic;

namespace Cwru.Common.Model
{
    public class ProjectInfo
    {
        public Guid Guid { get; set; }
        public string Root { get; set; }
        public IEnumerable<string> Files { get; set; }
        public IEnumerable<string> SelectedFiles { get; set; }
        public string SelectedFile { get; set; }
    }
}
