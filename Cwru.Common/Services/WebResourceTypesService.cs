using Cwru.Common.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Cwru.Common.Services
{
    public class WebResourceTypesService
    {
        private readonly ILogger logger;

        public WebResourceTypesService(ILogger logger)
        {
            this.logger = logger;
        }

        private static Dictionary<string, WebResourceType> extensionToTypeMapping = new Dictionary<string, WebResourceType>
        {
            { ".htm", WebResourceType.Html },
            { ".html", WebResourceType.Html },
            { ".css", WebResourceType.StyleSheet },
            { ".js" , WebResourceType.JScript },
            { ".xml", WebResourceType.Xml },
            { ".png", WebResourceType.Png },
            { ".jpg", WebResourceType.Jpg },
            { ".jpeg", WebResourceType.Jpg },
            { ".gif", WebResourceType.Gif },
            { ".xap", WebResourceType.Xap },
            { ".xsl", WebResourceType.Xsl },
            { ".ico", WebResourceType.Ico },
            { ".svg", WebResourceType.Svg },
            { ".resx", WebResourceType.Resx }
        };

        public IEnumerable<string> GetExtensions(WebResourceType type)
        {
            return
                extensionToTypeMapping.
                GroupBy(k => k.Value, v => v.Key).
                FirstOrDefault(x => x.Key == type).
                ToList();
        }

        public WebResourceType? GetTypeByExtension(string extension)
        {
            return extensionToTypeMapping.ContainsKey(extension) ? extensionToTypeMapping[extension] : (WebResourceType?)null;
        }

        public Dictionary<WebResourceType, string> GetTypesLabels()
        {
            return Enum.GetValues(typeof(WebResourceType)).
                Cast<WebResourceType>().
                ToDictionary(k => k, v =>
                {
                    var fi = v.GetType().GetField(v.ToString());

                    var attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

                    return attributes != null && attributes.Any() ?
                        attributes.First().Description :
                        v.ToString();
                });
        }

        public WebResourceType? GetTypeByLabel(string wrTypeLabelToShow)
        {
            if (string.IsNullOrWhiteSpace(wrTypeLabelToShow))
            {
                return null;
            }

            var typesLabels = GetTypesLabels();
            return typesLabels.Where(x => x.Value == wrTypeLabelToShow).Select(x => x.Key).FirstOrDefault();
        }
    }
}
