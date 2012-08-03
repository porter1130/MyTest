using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CA
{
    class TRWorkflowFactory : IWorkflowFactory
    {
        public BaseWF CreateWF(MOSSContext context)
        {
            return new TravelRequestWF(context);
        }
    }
}
