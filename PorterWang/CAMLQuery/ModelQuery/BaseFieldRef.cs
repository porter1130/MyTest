using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PorterWang.CAMLQuery
{
    public interface IFieldRef
    {
        string FieldName { get; }
    }
    public abstract class BaseFieldRef<T> : IFieldRef where T : class
    {
        private string _fieldName;
        public string FieldName
        {
            get { return _fieldName; }
        }

        public BaseFieldRef(string name)
        {
            _fieldName = name;
        }

        protected virtual CAMLExpression<T> CreateExpression(string op, object value)
        {
            SingleCAMLExpression<T> q = new SingleCAMLExpression<T>(_fieldName, op, value);
            return q;
        }

        #region operator methods
        public CAMLExpression<T> IsNull
        {
            get
            {
                return CreateExpression(ComparisonOperators.IsNull, null);
            }
        }

        public CAMLExpression<T> IsNotNull
        {
            get
            {
                return CreateExpression(ComparisonOperators.IsNotNull, null);
            }
        }
        #endregion

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
