using System;
using System.Collections.Generic;
using System.Linq;

namespace Cwru.Publisher.Extensions
{
    public static class ListExtensions
    {
        public static IEnumerable<string> ExcludeFile(this IEnumerable<string> list, string fileName)
        {
            return list?.Where(x => !x.EndsWith(fileName, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }
    }
}
