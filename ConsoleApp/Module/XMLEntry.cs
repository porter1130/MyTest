using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;
using ConsoleApp.Object;

namespace ConsoleApp.Module
{
    public class XMLEntry
    {
        public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
        {
            #region Fields
            private Dictionary<string, XmlSerializer> XmlSerializers;
            #endregion

            #region Method
            public SerializableDictionary()
            {
                this.XmlSerializers = new Dictionary<string, XmlSerializer>();
            }

            public XmlSchema GetSchema()
            {
                return null;
            }

            public void ReadXml(XmlReader reader)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TKey));
                XmlSerializer xmlSerializer = null;
                bool isEmptyElement = reader.IsEmptyElement;
                reader.Read();
                if (!isEmptyElement)
                {
                    while (reader.NodeType != XmlNodeType.EndElement)
                    {
                        string attribute = reader.GetAttribute("type");
                        reader.ReadStartElement("item");
                        xmlSerializer = this.GetXmlSerializer(attribute);

                        reader.ReadStartElement("key");
                        TKey key = (TKey)serializer.Deserialize(reader);
                        reader.ReadEndElement();

                        reader.ReadStartElement("value");
                        TValue value = (TValue)xmlSerializer.Deserialize(reader);
                        reader.ReadEndElement();

                        base.Add(key, value);
                        reader.ReadEndElement();
                        reader.MoveToContent();
                    }
                    reader.ReadEndElement();
                }
            }

            public void WriteXml(XmlWriter writer)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TKey));

                foreach (TKey key in base.Keys)
                {
                    writer.WriteStartElement("item");
                    TValue value = base[key];
                    string assemblyQualifiedName = value.GetType().AssemblyQualifiedName;
                    writer.WriteAttributeString("type", assemblyQualifiedName);

                    writer.WriteStartElement("key");
                    serializer.Serialize(writer, key);
                    writer.WriteEndElement();

                    writer.WriteStartElement("value");
                    this.GetXmlSerializer(assemblyQualifiedName).Serialize(writer, value);
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

                XmlSerializer serializer = new XmlSerializer(Type.GetType(type));
                this.XmlSerializers[type] = serializer;
                return serializer;
            }
            #endregion





        }

        public static class SerializeUtil
        {
            public static string Serialize(object obj)
            {
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                MemoryStream m = new MemoryStream();
                XmlTextWriter writer = new XmlTextWriter(m, Encoding.UTF8);
                serializer.Serialize((XmlWriter)writer, obj);
                return Encoding.UTF8.GetString(m.ToArray()).Trim();
            }
        }

        internal static void SerializeWorkflowVariable(Microsoft.SharePoint.SPWeb web)
        {
            WorkflowVariableValues vs = new WorkflowVariableValues();
            vs["test1"] = "1";
            vs["test2"] = true;

            SerializeUtil.Serialize(vs);
        }
    }


}
