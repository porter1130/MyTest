using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace PorterWang.CAMLQuery
{
    public interface IFieldInternalNameProvider
    {
        string GetInternalName(string displayName);
    }

    public interface ICAMLExpression
    {
        void ToCAML(IFieldInternalNameProvider provider, XmlNode pNode);
        void ToCAML(XmlNode pNode);
    }

    public abstract class CAMLExpression<T> : ICAMLExpression
    {
        #region ICAMLExpression Members

        public abstract void ToCAML(IFieldInternalNameProvider provider, XmlNode parentNode);

        public virtual void ToCAML(XmlNode pNode)
        {
            ToCAML(null, pNode);
        }

        public override string ToString()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<Where/>");

            this.ToCAML(null, doc.DocumentElement);

            return doc.InnerXml;
        }

        #endregion

        private CAMLExpression<T> And(CAMLExpression<T> q)
        {
            return new JoinCAMLExpression<T>(LogicalJoin.And, this, q);
        }
        private CAMLExpression<T> Or(CAMLExpression<T> q)
        {
            return new JoinCAMLExpression<T>(LogicalJoin.Or, this, q);

        }
        public static CAMLExpression<T> operator &(CAMLExpression<T> q1, CAMLExpression<T> q2)
        {
            return q1.And(q2);
        }
        public static CAMLExpression<T> operator |(CAMLExpression<T> q1, CAMLExpression<T> q2)
        {
            return q1.Or(q2);
        }

        public static bool operator true(CAMLExpression<T> right)
        {
            return false;
        }
        public static bool operator false(CAMLExpression<T> right)
        {
            return false;
        }

    }


}
