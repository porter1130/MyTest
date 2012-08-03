using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace CA
{
    class SerializeUtil
    {
        public static string Serialize(object obj) {
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            MemoryStream w = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(w, Encoding.UTF8);
            serializer.Serialize((XmlWriter)writer, obj);
            return Encoding.UTF8.GetString(w.ToArray()).Trim();
        }
    }
}
