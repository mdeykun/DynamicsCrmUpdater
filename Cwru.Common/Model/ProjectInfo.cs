using System;
using System.Collections.Generic;

namespace Cwru.Common.Model
{
    public class ProjectInfo
    {
        public Guid Guid { get; set; }
        public string Root { get; set; }
        public IEnumerable<SolutionElement> Elements { get; set; }
        public IEnumerable<SolutionElement> ElementsFlat { get; set; }
        public IEnumerable<SolutionElement> SelectedElements { get; set; }
    }
}
