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
            SPListItem item = context.CurrList.GetItemById(itemId);

            string eventData = @"<?xml version='1.0' encoding='utf-8'?><Data><item type='QuickFlow.NameCollection, QuickFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ec1e0fe6e1745628'><key><string>CheckTaskUsers</string></key><value><ArrayOfString xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><string>CNAIDC\Cynthia.Sun</string><string>CNAIDC\Lingling.Zhou</string><string>CNAIDC\Debbie.Yang</string></ArrayOfString></value></item><item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>IsSave</string></key><value><boolean>false</boolean></value></item><item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>IsSkipApprove</string></key><value><boolean>false</boolean></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>CompleteTaskTitle</string></key><value><string>PR001143 :Please complete the PR</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>CheckTaskTitle</string></key><value><string>Vivian Yang's Purchase Request  'PR001143' needs check</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>ApproveTaskTitle</string></key><value><string>Vivian Yang's Purchase Request  'PR001143' needs approval</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>ConfirmTaskTitle</string></key><value><string>Vivian Yang's Purchase Request  'PR001143' needs generate PO</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>CompleteTaskFormURL</string></key><value><string>/_Layouts/CA/WorkFlows/PurchaseRequest/EditForm.aspx</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>CheckTaskFormURL</string></key><value><string>/_Layouts/CA/WorkFlows/PurchaseRequest/CheckForm.aspx</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>ApproveTaskFormURL</string></key><value><string>/_Layouts/CA/WorkFlows/PurchaseRequest/ApproveForm.aspx</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>ConfirmTaskFormURL</string></key><value><string>/_Layouts/CA/WorkFlows/PurchaseRequest/ConfirmForm.aspx</string></value></item><item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>IsHO</string></key><value><boolean>false</boolean></value></item></Data>";

            using (SPSite osite = new SPSite(context.Currweb.Site.ID, context.Currweb.Site.UserToken))
            {
                using (SPWeb oweb = osite.OpenWeb(context.Currweb.ID))
                {
                    SPWorkflowAssociation wfAss = context.CurrList.WorkflowAssociations.GetAssociationByName(context.WfName, CultureInfo.CurrentCulture);

                    osite.WorkflowManager.StartWorkflow(item, wfAss, eventData);
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

            string eventData = @"<?xml version='1.0' encoding='utf-8'?><Data><item type='QuickFlow.NameCollection, QuickFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ec1e0fe6e1745628'><key><string>ApproveTaskUsers</string></key><value><ArrayOfString xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><string>CNAIDC\Marco.Hamers</string><string>CNAIDC\vivian.shen</string></ArrayOfString></value></item><item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>IsContinue</string></key><value><boolean>true</boolean></value></item></Data>";

            #region alter task
            Hashtable hash = new Hashtable();
            hash.Add("__WorkflowVaribales", eventData);
            //hash.Add("__TaskOutcome", "Confirm");
            hash.Add("__TaskOutcome", "Approve");
            hash.Add("__Action", "Commit");

            SPWorkflowTask.AlterTask(taskItem, hash, false);
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
                    ht.Add(SPBuiltInFieldId.WorkflowOutcome, string.Empty);
                    ht.Add("Status", SPUtility.GetLocalizedString("$Resources:Tasks_NotStarted;", "core", context.Currweb.Language));
                    ht.Add("__ReAssignUser", context.Currweb.Users.GetByID(reassignUserId));
                    ht.Add("__Action", "ReAssign");

                    SPWorkflowTask.AlterTask(task as SPListItem, ht, true);
                    context.Currweb.AllowUnsafeUpdates = false;
                    Console.WriteLine("Successful!");
                }
            }
        }
    }
}
