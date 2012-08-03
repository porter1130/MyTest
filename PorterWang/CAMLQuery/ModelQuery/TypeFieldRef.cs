using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PorterWang.CAMLQuery
{
    public class TypeFieldRef<TEntity, TField>:BaseFieldRef<TEntity> where TEntity:class,new()
    {
        public TypeFieldRef(string name):base(name) { }

        protected override CAMLExpression<TEntity> CreateExpression(string op, object value)
        {
            SingleCAMLExpression<TEntity> q = new SingleCAMLExpression<TEntity>(this.FieldName, op, value, typeof(TField));
            return q;
        }

        #region operator methods
        public CAMLExpression<TEntity> Equal(object value)
        {
            return CreateExpression(ComparisonOperators.Equal, value);
        }

        public CAMLExpression<TEntity> NotEqual(object value)
        {
            return CreateExpression(ComparisonOperators.NotEqual, value);
        }

        public CAMLExpression<TEntity> BeginsWith(object value)
        {
            return CreateExpression(ComparisonOperators.BeginsWith, value);
        }

        public CAMLExpression<TEntity> Contains(object value)
        {
            return CreateExpression(ComparisonOperators.Contains, value);
        }

        public CAMLExpression<TEntity> LessThan(object value)
        {
            return CreateExpression(ComparisonOperators.LessThan, value);
        }

        public CAMLExpression<TEntity> MoreThan(object value)
        {
            return CreateExpression(ComparisonOperators.MoreThan, value);
        }

        public CAMLExpression<TEntity> LessEqual(object value)
        {
            return CreateExpression(ComparisonOperators.LessEqual, value);
        }

        public CAMLExpression<TEntity> MoreEqual(object value)
        {
            return CreateExpression(ComparisonOperators.MoreEqual, value);
        }
        #endregion

        #region operator overload
        public static CAMLExpression<TEntity> operator ==(TypeFieldRef<TEntity,TField> q, object oValue)
        {
            return q.Equal(oValue);
        }
        public static CAMLExpression<TEntity> operator !=(TypeFieldRef<TEntity, TField> q, object oValue)
        {
            return q.NotEqual(oValue);
        }
        public static CAMLExpression<TEntity> operator >(TypeFieldRef<TEntity, TField> q, object oValue)
        {
            return q.MoreThan(oValue);
        }
        public static CAMLExpression<TEntity> operator <(TypeFieldRef<TEntity, TField> q, object oValue)
        {
            return q.LessThan(oValue);
        }
        public static CAMLExpression<TEntity> operator >=(TypeFieldRef<TEntity, TField> q, object oValue)
        {
            return q.MoreEqual(oValue);
        }
        public static CAMLExpression<TEntity> operator <=(TypeFieldRef<TEntity, TField> q, object oValue)
        {
            return q.LessEqual(oValue);
        }
        #endregion

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
