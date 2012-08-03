using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace PorterWang.CAMLQuery
{
    public class QueryContext
    {
        internal QueryContext() { }

        public SPList List{get;set;}

        public string ListName { get; set; }

        public IFieldRef[] ViewFields { get; set; }

        public uint RowLimit { get; set; }

        public ICAMLExpression Query { get; set; }

        public IDictionary<IFieldRef, bool> OrderByFields = new Dictionary<IFieldRef, bool>();

        public IFieldRef GroupByField { get; set; }
    }

    
}
