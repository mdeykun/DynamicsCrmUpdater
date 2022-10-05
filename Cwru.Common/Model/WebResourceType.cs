using System.ComponentModel;

namespace Cwru.Common.Model
{
    public enum WebResourceType
    {
        [Description("Webpage (HTML)")]
        Html = 1,

        [Description("Stylesheet (CSS)")]
        StyleSheet = 2,

        [Description("Script (JScript)")]
        JScript = 3,

        [Description("Data (XML)")]
        Xml = 4,

        [Description("Image (PNG)")]
        Png = 5,

        [Description("Image (JPG)")]
        Jpg = 6,

        [Description("Image (GIF)")]
        Gif = 7,

        [Description("Silverlight (XAP)")]
        Xap = 8,

        [Description("Stylesheet (XSL)")]
        Xsl = 9,

        [Description("Image (ICO)")]
        Ico = 10,

        [Description("Vector format (SVG)")]
        Svg = 11,

        [Description("String (RESX)")]
        Resx = 12,
    }
}
