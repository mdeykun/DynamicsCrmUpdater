using System;
using System.IO;
using System.Xml.Serialization;

namespace Cwru.Common.Services
{
    public class XmlSerializerService
    {
        public object Deserialize(string xmlContent, Type objectType)
        {
            StringReader reader = new StringReader(xmlContent);
            XmlSerializer s = new XmlSerializer(objectType);
            return s.Deserialize(reader);
        }
    }
}