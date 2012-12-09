using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;
using System.Collections;
using Microsoft.SharePoint.Utilities;
using System.Globalization;

namespace CA
{
    class CashAdvanceWF : BaseWF
    {
        public CashAdvanceWF(MOSSContext context)
            : base(context)
        {
            context.CurrList = context.GetListByName(SPListName.CashAdvanceRequest);
        }
        public override void StartWorkflowInstance(int itemId)
        {
            MOSSContext context = base._context;

            string eventData = @"<?xml version='1.0' encoding='utf-8'?><Data><item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>IsFinance</string></key><value><boolean>false</boolean></value></item><item type='QuickFlow.NameCollection, QuickFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ec1e0fe6e1745628'><key><string>NextApproveTaskUsers</string></key><value><ArrayOfString xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><string>CNAIDC\Yannie.Zhang</string></ArrayOfString></value></item><item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>IsContinue</string></key><value><boolean>true</boolean></value></item><item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>IsSave</string></key><value><boolean>false</boolean></value></item><item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>IsUrgent</string></key><value><boolean>false</boolean></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>CompleteTaskFormURL</string></key><value><string>/_Layouts/CA/WorkFlows/CashAdvanceRequest/EditForm.aspx</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>NextApproveTaskFormURL</string></key><value><string>/_Layouts/CA/WorkFlows/CashAdvanceRequest/ApproveForm.aspx</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>NextApproveTaskTitle</string></key><value><string>CA_000058 16500 Simon Zhang's Cash Advance Request needs approval</string></value></item></Data>";

            using (SPSite osite = new SPSite(context.Currweb.Site.ID, context.ApplicantSPUser.UserToken))
            {
                using (SPWeb oweb = osite.OpenWeb(context.Currweb.ID))
                {
                    SPList olist = oweb.Lists[context.CurrList.Title];

                    SPListItem oitem = olist.GetItemById(itemId);

                    SPWorkflowAssociation wfAss = olist.WorkflowAssociations.GetAssociationByName(context.WfName, CultureInfo.CurrentCulture);

                    osite.WorkflowManager.StartWorkflow(oitem, wfAss, eventData);
                }
            }

        }

        public override void RetryingWF(int itemId)
        {
            MOSSContext context = base._context;
            SPListItem taskItem = context.Currweb.Lists["Tasks"].GetItemById(itemId);

            string eventData = @"<?xml version='1.0' encoding='utf-8'?><Data><item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>IsContinue</string></key><value><boolean>false</boolean></value></item><item type='QuickFlow.NameCollection, QuickFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ec1e0fe6e1745628'><key><string>NextApproveTaskUsers</string></key><value><ArrayOfString xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><string>CNAIDC\poppy.wang</string></ArrayOfString></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>NextApproveTaskTitle</string></key><value><string>CA_000058 16500 CNAIDC\Simon.Zhang's Cash Advance Request needs confirm</string></value></item></Data>";

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
            MOSSContext context = base._context;

            SPListItem item = context.CurrList.GetItemById(itemId);

            foreach (SPWorkflowTask task in item.Tasks)
            {
                if (task.ID.Equals(taskId))
                {
                    context.Currweb.AllowUnsafeUpdates = true;
                    Hashtable ht = new Hashtable();
                    //ht.Add("AssignedTo",web.EnsureUser(@"micro\ituser"));
                    ht.Add(SPBuiltInFieldId.WorkflowOutcome, string.Empty);
                    ht.Add("Status", SPUtility.GetLocalizedString("$Resources:Tasks_NotStarted;", "core", context.Currweb.Language));
                    ht.Add("__ReAssignUser", context.Currweb.Users.GetByID(reassignUserId));
                    ht.Add("__Action", "ReAssign");

                    SPWorkflowTask.AlterTask(task as SPListItem, ht, true);
                    Console.WriteLine("Successful!");
                }
            }
        }
    }


}
