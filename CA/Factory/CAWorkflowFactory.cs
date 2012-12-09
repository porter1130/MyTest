using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CA
{

    class CAWorkflowFactory : IWorkflowFactory
    {
        public BaseWF CreateWF(MOSSContext context)
        {
            return new CashAdvanceWF(context);
        }
    }
}
