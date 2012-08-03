using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace PorterWang.CAMLQuery
{
    public class JoinCAMLExpression<T> : CAMLExpression<T>
    {
        public readonly LogicalJoin LogicalJoin;

        public readonly CAMLExpression<T> Left;
        public readonly CAMLExpression<T> Right;

        public JoinCAMLExpression(LogicalJoin join, CAMLExpression<T> left, CAMLExpression<T> right)
        {
            LogicalJoin = join;
            Left = left;
            Right = right;
        }

        public override void ToCAML(IFieldInternalNameProvider provider, System.Xml.XmlNode parentNode)
        {
            XmlElement node = parentNode.OwnerDocument.CreateElement(LogicalJoin.ToString());

            parentNode.AppendChild(node);

            Left.ToCAML(provider, node);
            Right.ToCAML(provider, node);
        }

    }

    public enum LogicalJoin
    {
        And,
        Or
    }
}
