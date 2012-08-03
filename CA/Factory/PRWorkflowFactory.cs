using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace CA
{
    class PRWorkflowFactory : IWorkflowFactory
    {
        public BaseWF CreateWF(MOSSContext context)
        {
            return new PurchaseRequestWF(context);
        }
    }
}
