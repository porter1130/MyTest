﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;

namespace PorterWang.CAMLQuery
{
    public class FieldInternalNameProvider : IFieldInternalNameProvider
    {
        private SPList _list;

        public FieldInternalNameProvider(SPList list)
        {
            _list = list;
        }

        public string GetInternalName(string displayName)
        {
            SPField f = _list.Fields.GetField(displayName);

            if (f == null)
            {
                throw new Exception(string.Format("Field[{0}] doesn't exist."));
            }

            return f.InternalName;
        }
    }

    public static class CAMLBuilder
    {
        public static string Where(ICAMLExpression expr)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"<Where/>");

            if (expr != null)
            {
                expr.ToCAML(doc.DocumentElement);
            }

            return doc.InnerXml;
        }

        public static string Where(ICAMLExpression expr, SPList list)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"<Where/>");

            FieldInternalNameProvider p = new FieldInternalNameProvider(list);

            if (expr != null)
            {
                expr.ToCAML(p, doc.DocumentElement);
            }

            return doc.InnerXml;

        }

        public static String ViewFields(params IFieldRef[] fields)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"<Where/>");

            foreach (IFieldRef f in fields)
            {
                XmlNode fNode = doc.CreateElement("FieldRef");
                doc.DocumentElement.AppendChild(fNode);

                XmlAttribute att = doc.CreateAttribute("Name");
                att.Value = f.FieldName;
                fNode.Attributes.Append(att);
            }

            return doc.DocumentElement.InnerXml;
        }

        public static string OrderBy(IDictionary<IFieldRef, bool> fields)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"<OrderBy/>");

            foreach (KeyValuePair<IFieldRef, bool> kv in fields)
            {
                XmlNode fNode = doc.CreateElement("FieldRef");
                doc.DocumentElement.AppendChild(fNode);

                XmlAttribute att = doc.CreateAttribute("Name");
                att.Value = kv.Key.FieldName;
                fNode.Attributes.Append(att);

                if (kv.Value == false)
                {
                    att = doc.CreateAttribute("Ascending");
                    att.Value = "FALSE";
                    fNode.Attributes.Append(att);
                }
            }

            return doc.InnerXml;
        }

        public static string View(QueryContext qc)
        {
            string xml = "<View>" +
                        "<Query>" +
                        Where(qc.Query);

            if (qc.OrderByFields.Count > 0)
            {
                xml += OrderBy(qc.OrderByFields);
            }

            xml += "</Query>";

            if (qc.ViewFields != null && qc.ViewFields.Length > 0)
            {
                xml += "<ViewFields>" + ViewFields(qc.ViewFields) + "</ViewFields>";
            }

            if (qc.RowLimit > 0)
            {
                xml += "<RowLimit>" + qc.RowLimit + "</RowLimit>";
            }


            xml += "</View>";

            return xml;
        }
    }


}
