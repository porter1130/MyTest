using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PorterWang.CAMLQuery
{

    public class TypeQueryField<TField> : TypeFieldRef<object, TField>
    {
        public TypeQueryField(string name) : base(name) { }
    }
}
