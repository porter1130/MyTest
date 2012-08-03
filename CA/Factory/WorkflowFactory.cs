using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CA
{
    class WorkflowFactory : IWorkflowFactory
    {
        public BaseWF CreateWF(MOSSContext context)
        {
            return new WF(context);
        }
    }
}
