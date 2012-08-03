using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace CA
{
    interface IWorkflowFactory
    {
        BaseWF CreateWF(MOSSContext context);
    }
}
