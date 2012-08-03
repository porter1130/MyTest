using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;
using PorterWang.CommonUtil;
using System.Globalization;
using System.Collections;
using Microsoft.SharePoint.Utilities;

namespace CA
{
    class PaymentRequestWF : BaseWF
    {
        public PaymentRequestWF(MOSSContext context)
            : base(context)
        {
        }

        public override void StartWorkflowInstance(int itemId)
        {
            MOSSContext context = base._context;
            SPListItem item = context.CurrList.GetItemById(itemId);

            string eventData = @"<?xml version='1.0' encoding='utf-8'?><Data><item type='QuickFlow.NameCollection, QuickFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ec1e0fe6e1745628'><key><string>ConfirmTaskUsers</string></key><value><ArrayOfString xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><string>CNAIDC\ella.huang</string></ArrayOfString></value></item><item type='QuickFlow.NameCollection, QuickFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ec1e0fe6e1745628'><key><string>ApproveTaskUsers</string></key><value><ArrayOfString xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><string>CNAIDC\ella.huang</string></ArrayOfString></value></item><item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>IsSave</string></key><value><boolean>false</boolean></value></item><item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>IsConfirm</string></key><value><boolean>false</boolean></value></item><item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>IsContinue</string></key><value><boolean>false</boolean></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>CompleteTaskFormUrl</string></key><value><string>/_Layouts/CA/WorkFlows/PaymentRequest/EditForm.aspx</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>ApproveTaskFormUrl</string></key><value><string>/_Layouts/CA/WorkFlows/PaymentRequest/ApproveForm.aspx</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>ConfirmTaskFormUrl</string></key><value><string>/_Layouts/CA/WorkFlows/PaymentRequest/ApproveForm.aspx</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>ApproveTaskTitle</string></key><value><string>PR00000840_1 上海珺才企... 181497.9 Rena Yin's Payment Request needs approval</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>ConfirmTaskTitle</string></key><value><string>PR00000840_1 上海珺才企... 181497.9 Rena Yin's Payment Request needs confirm</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>CompleteTaskTitle</string></key><value><string>Please complete Payment Request</string></value></item></Data>";

            using (SPSite osite = new SPSite(context.Currweb.Site.ID, context.Currweb.Site.UserToken))
            {
                using (SPWeb oweb = osite.OpenWeb(context.Currweb.ID))
                {
                    SPWorkflowAssociation wfAss = context.CurrList.WorkflowAssociations.GetAssociationByName(context.WfName, CultureInfo.CurrentCulture);

                    osite.WorkflowManager.StartWorkflow(item, wfAss, eventData);
                }
            }
        }

        public override void RetryingWF(int itemId)
        {
            MOSSContext context = base._context;
            SPListItem taskItem = context.Currweb.Lists["Tasks"].GetItemById(itemId);

            string eventData = @"<?xml version='1.0' encoding='utf-8'?><Data><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>ApproveTaskTitle</string></key><value><string>PR00000840_1 上海珺才企... 181497.9 Rena Yin(CNAIDC\rena.yin)'s Payment Request needs approval</string></value></item><item type='QuickFlow.NameCollection, QuickFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ec1e0fe6e1745628'><key><string>ApproveTaskUsers</string></key><value><ArrayOfString xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><string>CNAIDC\Mark.Siezen</string></ArrayOfString></value></item><item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>IsContinue</string></key><value><boolean>true</boolean></value></item><item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>IsReject</string></key><value><boolean>false</boolean></value></item></Data>";

            #region alter task
            Hashtable hash = new Hashtable();
            hash.Add("__WorkflowVaribales", eventData);
            hash.Add("__TaskOutcome", "Approve");
            hash.Add("__Action", "Commit");

            SPWorkflowTask.AlterTask(taskItem, hash, true);
            #endregion
        }

        public override void ReassignWorkflowTask(int itemId, int taskId, int reassignUserId)
        {
            throw new NotImplementedException();

        }
    }
}
