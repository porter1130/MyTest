using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CA
{
    class PaymentRequestFactory : IWorkflowFactory
    {
        public BaseWF CreateWF(MOSSContext context)
        {
            return new PaymentRequestWF(context);
        }
    }
}
