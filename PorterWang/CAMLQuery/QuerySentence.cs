using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace PorterWang.CAMLQuery
{
    public abstract class QuerySentence
    {
        protected readonly QueryContext Context;

        internal QuerySentence(QueryContext context)
        {
            Context = context;
        }

        public string ToCAMLString()
        {
            return CAMLBuilder.View(this.Context);
        }
    }

    public abstract class ReturnableQuerySentence : QuerySentence
    {
        public ReturnableQuerySentence(QueryContext c) : base(c) { }

        public SPListItemCollection GetItems()
        {
            SPQuery query = new SPQuery();

            query.ViewXml = CAMLBuilder.View(this.Context);

            SPListItemCollection items = Context.List.GetItems(query);

            return items;
        }
    }


    public class Select : QuerySentence
    {
        internal Select(QueryContext context, params IFieldRef[] fields)
            : base(context)
        {
            context.ViewFields = fields;
        }

        public From From(SPList list)
        {
            From from = new From(this.Context, list);
            return from;
        }
    }

    public class From : QuerySentence
    {

        internal From(QueryContext context, SPList list)
            : base(context)
        {
            context.List = list;
        }

        public Where Where(ICAMLExpression q)
        {
            Where where = new Where(this.Context, q);

            return where;
        }

        public Order OrderBy(IFieldRef field)
        {
            return new Order(this.Context, field, true);
        }

        public Order OrderBy(IFieldRef field, bool desc)
        {
            return new Order(this.Context, field, desc);
        }
    }

    public class Where : ReturnableQuerySentence
    {
        internal Where(QueryContext context, ICAMLExpression expr)
            : base(context)
        {
            context.Query = expr;
        }
    }

    public class Order : ReturnableQuerySentence
    {
        internal Order(QueryContext context, IFieldRef field, bool desc)
            : base(context)
        {
            context.OrderByFields.Add(field, desc);
        }

        public Order OrderBy(IFieldRef field, bool desc)
        {
            this.Context.OrderByFields.Add(field, desc);
            return this;
        }
    }

    public class Group : ReturnableQuerySentence
    {
        internal Group(QueryContext context, IFieldRef field)
            : base(context)
        {
            context.GroupByField = field;
        }
    }


    public static class ListQuery
    {
        public static Select Select(params IFieldRef[] fields)
        {
            QueryContext context = new QueryContext();

            Select s = new Select(context, fields);

            return s;
        }

        public static Select Select(uint top, params IFieldRef[] fields)
        {
            QueryContext context = new QueryContext();
            context.RowLimit = top;

            Select s = new Select(context, fields);

            return s;
        }

        public static Select Select(uint top)
        {
            QueryContext context = new QueryContext();
            context.RowLimit = top;

            Select s = new Select(context, null);

            return s;
        }

        public static Select Select()
        {
            QueryContext context = new QueryContext();

            Select s = new Select(context, null);
            return s;
        }
    }
}
