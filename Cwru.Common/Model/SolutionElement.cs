using System.Collections.Generic;

namespace Cwru.Common.Model
{
    public class SolutionElement
    {
        public SolutionElement()
        {
            Childs = new List<SolutionElement>();
        }

        public List<SolutionElement> Childs { get; set; }

        public SolutionElementType Type { get; set; }

        public string FilePath { get; set; }

        //public bool IsSelected { get; set; }
    }
}
