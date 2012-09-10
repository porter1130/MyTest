using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Microsoft.SharePoint.Workflow;
using System.Globalization;
using PorterWang.CommonUtil;
using System.Collections;
using Microsoft.SharePoint.Utilities;
using System.Reflection;
using QuickFlow.Tracing;

namespace CA
{
    class PurchaseRequestWF : BaseWF
    {

        public PurchaseRequestWF(MOSSContext context)
            : base(context)
        {
        }

        public override void StartWorkflowInstance(int itemId)
        {
            MOSSContext context = base._context;

            string eventData = @"<?xml version='1.0' encoding='utf-8'?><Data><item type='QuickFlow.NameCollection, QuickFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ec1e0fe6e1745628'><key><string>CheckTaskUsers</string></key><value><ArrayOfString xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><string>CNAIDC\Cynthia.Sun</string><string>CNAIDC\Lingling.Zhou</string><string>CNAIDC\Debbie.Yang</string></ArrayOfString></value></item><item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>IsSave</string></key><value><boolean>false</boolean></value></item><item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>IsSkipApprove</string></key><value><boolean>false</boolean></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>CompleteTaskTitle</string></key><value><string>PR001164 :Please complete the PR</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>CheckTaskTitle</string></key><value><string>Stella Piao's Purchase Request  'PR001164' needs check</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>ApproveTaskTitle</string></key><value><string>Stella Piao's Purchase Request  'PR001164' needs approval</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>ConfirmTaskTitle</string></key><value><string>Stella Piao's Purchase Request  'PR001164' needs generate PO</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>CompleteTaskFormURL</string></key><value><string>/_Layouts/CA/WorkFlows/PurchaseRequest/EditForm.aspx</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>CheckTaskFormURL</string></key><value><string>/_Layouts/CA/WorkFlows/PurchaseRequest/CheckForm.aspx</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>ApproveTaskFormURL</string></key><value><string>/_Layouts/CA/WorkFlows/PurchaseRequest/ApproveForm.aspx</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>ConfirmTaskFormURL</string></key><value><string>/_Layouts/CA/WorkFlows/PurchaseRequest/ConfirmForm.aspx</string></value></item><item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>IsHO</string></key><value><boolean>false</boolean></value></item></Data>";

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
            //if (item != null)
            //{
            //    context.CurrItem = item;
            //    Regex r = new Regex(@"^.*\((.*)\)$");
            //    string loginName = r.Match(item["Applicant"].ToString()).Groups[1].ToString();
            //    context.ApplicantSPUser = context.Currweb.AllUsers[loginName];

            //    BaseTaskStep taskStep = new BaseTaskStep(context);
            //    context.UpdateWorkflowVariable("CompleteTaskFormURL", taskStep.EditUrl);
            //    context.UpdateWorkflowVariable("CheckTaskFormURL", taskStep.CheckUrl);
            //    context.UpdateWorkflowVariable("ApproveTaskFormURL", taskStep.ApproveUrl);
            //    context.UpdateWorkflowVariable("ConfirmTaskFormURL", taskStep.ConfirmUrl);

            //    context.UpdateWorkflowVariable("CompleteTaskTitle", taskStep.CompleteTaskTilte);
            //    context.UpdateWorkflowVariable("CheckTaskTitle", taskStep.CheckTaskTilte);
            //    context.UpdateWorkflowVariable("ApproveTaskTitle", taskStep.ApproveTaskTilte);
            //    context.UpdateWorkflowVariable("ConfirmTaskTitle", taskStep.ConfirmTaskTilte);

            //    context.UpdateWorkflowVariable("IsSave", false);
            //    context.UpdateWorkflowVariable("IsSkipApprove", false);
            //    context.UpdateWorkflowVariable("IsHO", false);
            //    context.UpdateWorkflowVariable("IsContinue", false);

            //    StringCollection checkTaskUsers = new StringCollection();
            //    checkTaskUsers.Add(context.Currweb.SiteUsers.GetByID(422).LoginName);
            //    //checkTaskUsers.Add(context.Currweb.SiteUsers.GetByID(581).LoginName);

            //    StringCollection approveTaskUsers = new StringCollection();
            //    checkTaskUsers.Add(context.Currweb.SiteUsers.GetByID(422).LoginName);

            //    context.UpdateWorkflowVariable("CheckTaskUsers", checkTaskUsers);
            //    context.UpdateWorkflowVariable("ApproveTaskUsers", approveTaskUsers);

            //    string eventData = context.SerializeWorkflowVariable();
            //    using (SPSite osite = new SPSite(context.Currweb.Site.ID, context.ApplicantSPUser.UserToken))
            //    {
            //        using (SPWeb oweb = osite.OpenWeb(context.Currweb.ID))
            //        {
            //            SPWorkflowAssociation wfAss = context.CurrList.WorkflowAssociations.GetAssociationByName(WorkflowName.PurchaseRequest1, CultureInfo.CurrentCulture);

            //            osite.WorkflowManager.StartWorkflow(context.CurrItem, wfAss, eventData);
            //        }
            //    }
            //}

        }

        public override void CancelWorkflowInstance(int itemId)
        {
            MOSSContext context = base._context;
            SPListItem item = context.CurrList.GetItemById(itemId);
            SPWorkflowManager wfManager = context.Currweb.Site.WorkflowManager;
            foreach (SPWorkflow workflow in wfManager.GetItemActiveWorkflows(item))
            {
                if (item.ParentList.WorkflowAssociations[workflow.AssociationId].Name.Equals(context.WfName, StringComparison.CurrentCultureIgnoreCase))
                {
                    //SPWorkflowManager.CancelWorkflow(workflow);
                    wfManager.RemoveWorkflowFromListItem(workflow);
                }
            }

        }

        public override void RetryingWF(int taskItemId)
        {
            MOSSContext context = base._context;
            SPListItem taskItem = context.Currweb.Lists["Tasks"].GetItemById(taskItemId);

            string eventData = @"<?xml version='1.0' encoding='utf-8'?><Data><item type='QuickFlow.NameCollection, QuickFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ec1e0fe6e1745628'><key><string>ApproveTaskUsers</string></key><value><ArrayOfString xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><string>CNAIDC\john.zhang</string></ArrayOfString></value></item></Data>";

            #region alter task
            Hashtable hash = new Hashtable();
            hash.Add("__WorkflowVaribales", eventData);
            //hash.Add("__TaskOutcome", "Submit");
            hash.Add("__TaskOutcome", "Confirm");
            //hash.Add("__TaskOutcome", "Approve");
            hash.Add("__Action", "Commit");

            SPWorkflowTask.AlterTask(taskItem, hash, true);
            #endregion
        }

        public override void ReassignWorkflowTask(int itemId, int taskItemId, int reassignUserId)
        {
            MOSSContext context = base._context;
            SPList list = context.CurrList;

            SPListItem item = list.GetItemById(itemId);

            foreach (SPWorkflowTask task in item.Tasks)
            {
                if (task.ID.Equals(taskItemId))
                {
                    context.Currweb.AllowUnsafeUpdates = true;
                    Hashtable ht = new Hashtable();
                    //ht.Add("AssignedTo",web.EnsureUser(@"micro\ituser"));
                    ht.Add(SPBuiltInFieldId.WorkflowOutcome, "Reassign");
                    ht.Add("__ReAssignUser", context.Currweb.Users.GetByID(reassignUserId).LoginName);
                    ht.Add("__Action", "ReAssign");

                    SPWorkflowTask.AlterTask(task as SPListItem, ht, true);
                    context.Currweb.AllowUnsafeUpdates = false;
                    Console.WriteLine("Successful!");
                }
            }
        }

        public override void UnLockSpecificWF(int itemId)
        {
            MOSSContext context = base._context;
            SPListItem wfItem = context.Currweb.Lists[SPListName.CashAdvanceRequest].GetItemById(itemId);
            //if (wfItem != null)
            //{
            //    foreach (SPWorkflow workflow in wfItem.Workflows)
            //    {
            //        Console.WriteLine(wfItem.ParentList.WorkflowAssociations[workflow.AssociationId].Name);
            //        foreach (SPWorkflowTask task in workflow.Tasks)
            //        {
            //            if (task[SPBuiltInFieldId.WorkflowVersion].AsString() != "1")
            //            {
            //                task[SPBuiltInFieldId.WorkflowVersion] = "1";
            //                task.SystemUpdate();
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    Console.WriteLine(string.Format("Sorry, the workflow number({0}) does not exist, please resubmit it.", itemId));
            //}



            SPWorkflow workflow = wfItem.Workflows[0];

            FlowchartWorkflowTracingService tracingService = new FlowchartWorkflowTracingService(context.Currweb, workflow.InstanceId);
            ActivityExecutionInfo activityExecutionInfo = tracingService.Get("start");
            
            //MethodInfo method = workflow.GetType().GetMethod("InstanceData", BindingFlags.NonPublic | BindingFlags.Instance);
            //method.Invoke(workflow, null);

            //FieldInfo field = workflow.GetType().GetField(" m_arrData", BindingFlags.NonPublic | BindingFlags.Instance);

            

            Hashtable extendedPropertiesAsHashtable = SPWorkflowTask.GetExtendedPropertiesAsHashtable(wfItem);

            foreach (DictionaryEntry entry in extendedPropertiesAsHashtable)
            {
                Console.WriteLine("Variable:{0}  Value:{1}", entry.Key.AsString(), entry.Value.AsString());
            }

        }
    }
}
