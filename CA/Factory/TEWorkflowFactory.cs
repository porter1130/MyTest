using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CA
{
    class TEWorkflowFactory : IWorkflowFactory
    {
        public BaseWF CreateWF(MOSSContext context)
        {
            return new TravelExpenseClaimWF(context);
        }
    }
}
