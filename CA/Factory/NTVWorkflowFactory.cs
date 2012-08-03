using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CA
{
    class NTVWorkflowFactory : IWorkflowFactory
    {
        public BaseWF CreateWF(MOSSContext context)
        {
            return new NonTradeWF(context);
        }
    }
}
