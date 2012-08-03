using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CA
{
    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        private Dictionary<string, XmlSerializer> XmlSerializers;

        public SerializableDictionary()
        {
            this.XmlSerializers = new Dictionary<string, XmlSerializer>();
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TKey));

            foreach (TKey local in base.Keys)
            {
                writer.WriteStartElement("item");
                TValue o = base[local];
                string assemblyQualifiedName = o.GetType().AssemblyQualifiedName;
                writer.WriteAttributeString("type", assemblyQualifiedName);

                writer.WriteStartElement("key");
                serializer.Serialize(writer, local);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                this.GetXmlSerializer(assemblyQualifiedName).Serialize(writer, o);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }

        private XmlSerializer GetXmlSerializer(string type)
        {
            if (this.XmlSerializers.ContainsKey(type))
            {
                return this.XmlSerializers[type];
            }
            else
            {
                XmlSerializer serializer = new XmlSerializer(Type.GetType(type));
                this.XmlSerializers[type] = serializer;
                return serializer;
            }
        }
    }
}
