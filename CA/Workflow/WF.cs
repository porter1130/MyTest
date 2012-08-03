using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;
using PorterWang.CAMLQuery;
using PorterWang.CommonUtil;
using System.Globalization;
using System.Collections;
using System.Threading;
using System.Web.UI;
using System.IO;
using System.Collections.Specialized;
using System.Xml;

namespace CA
{
    class WF : BaseWF
    {
        private static Dictionary<string, object> exceptionDict = new Dictionary<string, object>();
        private static List<int> excutedTaskList = new List<int>();
        private static MOSSContext context;


        public WF(MOSSContext context)
            : base(context)
        {
            WF.context = base._context;
            if (context.CurrList == null)
            {
                throw new Exception("The Current Context List is null, please set it.");
            }
        }

        #region Main Method
        public override void StartWorkflowInstance(int itemId)
        {
            throw new NotImplementedException();
        }

        public override bool FindLockedWorkflow()
        {
            bool hasLockedWF = false;
            Dictionary<int, string> taskLockDict = new Dictionary<int, string>(), exceptionLockDict = new Dictionary<int, string>();

            TypeQueryField<DateTime> queryField = new TypeQueryField<DateTime>("Created");

            CAMLExpression<object> exp = queryField.MoreEqual(DateTime.Now.AddDays(Constants.WF_Lastest_Days));

            SPListItemCollection items = ListQuery.Select()
                                                .From(context.CurrList)
                                                .Where(exp).GetItems();
            SPWorkflowManager wfManager = context.Currweb.Site.WorkflowManager;

            #region find Locked workflow list item
            foreach (SPListItem item in items)
            {
                foreach (SPWorkflow workflow in wfManager.GetItemActiveWorkflows(item))
                {
                    if (item.ParentList.WorkflowAssociations[workflow.AssociationId].Name.Equals(context.WfName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (workflow.IsLocked
                            || IsErrorOccured(item[context.CurrList.WorkflowAssociations[workflow.AssociationId].Name].AsString()))
                        {
                            RegisterLock(item, exceptionLockDict);
                            AddExceptionList(GetWFKey(item.ID, item.ParentList.Title), workflow.Tasks);
                            continue;
                        }

                        foreach (SPWorkflowTask task in workflow.Tasks)
                        {
                            if (task[SPBuiltInFieldId.WorkflowVersion].AsString() != "1")
                            {
                                RegisterLock(item, taskLockDict);
                                AddExceptionList(GetWFKey(item.ID, item.ParentList.Title), workflow.Tasks);
                                break;
                            }
                        }
                    }
                }

            }
            #endregion

            #region generate email notisfication
            context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
            context.HtmlWriter.Write("Scanning List {0}", context.CurrList.Title);
            context.HtmlWriter.WriteBreak();
            if (taskLockDict != null
                && taskLockDict.Count > 0)
            {
                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
                context.HtmlWriter.Write("There are {0} task locked item.", taskLockDict.Count);
                context.HtmlWriter.WriteBreak();
                foreach (KeyValuePair<int, string> kvp in taskLockDict)
                {
                    context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Href, kvp.Value);
                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
                    context.HtmlWriter.Write(" {0} ", kvp.Key);
                    context.HtmlWriter.RenderEndTag();
                }
                context.HtmlWriter.RenderEndTag();
                hasLockedWF = true;
            }
            else
            {
                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
                context.HtmlWriter.Write("There are not task locked items.");
                context.HtmlWriter.WriteBreak();
            }

            if (exceptionLockDict != null
                && exceptionLockDict.Count > 0)
            {
                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
                context.HtmlWriter.Write("There are {0} exception locked item.", exceptionLockDict.Count);
                context.HtmlWriter.WriteBreak();
                foreach (KeyValuePair<int, string> kvp in exceptionLockDict)
                {
                    context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Href, kvp.Value);
                    context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
                    context.HtmlWriter.Write(" {0} ", kvp.Key);
                    context.HtmlWriter.RenderEndTag();
                }
                context.HtmlWriter.RenderEndTag();
                hasLockedWF = true;
            }
            else
            {
                context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
                context.HtmlWriter.Write("There are not exception locked items.");
                context.HtmlWriter.WriteBreak();
            }
            context.HtmlWriter.RenderEndTag();
            #endregion

            return hasLockedWF;
        }

        private void RegisterLock(SPListItem item, Dictionary<int, string> lockDict)
        {
            if (!lockDict.ContainsKey(item.ID))
            {
                lockDict.Add(item.ID, string.Format("{0}?FilterField1={1}&FilterValue1={2}", context.Currweb.Site.Url + item.ParentList.Views["All Items"].ServerRelativeUrl, item.Fields[SPBuiltInFieldId.ID].InternalName, item.ID));
            }
        }

        public override void AutoUnLock()
        {
            bool hasLockedWF;
            using (StringWriter stringWriter = new StringWriter())
            {
                using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter, string.Empty))
                {
                    context.HtmlWriter = writer;

                    hasLockedWF = FindLockedWorkflow();
                    //UnLockWorkflow();
                    writer.RenderEndTag();
                }

                if (hasLockedWF)
                {
                    context.MailContent += stringWriter.ToString();
                }
            }
        }

        public override void SendAlertMail()
        {
            if (context.MailContent.IsNotNullOrWhitespace())
            {
                //ThreadPool.QueueUserWorkItem(delegate
                //{
                IMailService mailService = MailServiceFactory.GetMailService();
                StringCollection recipients = new StringCollection();
                recipients.Add("wang_jingchao@vanceinfo.com");
                //recipients.Add("huang_zhigang@vanceinfo.com");

                try
                {
                    mailService.SendMail("About EWF Locked Workflow", context.MailContent, recipients);
                }
                catch { }

                //});
            }
        }

        private void UnLockWorkflow()
        {
            Console.WriteLine("Start unlock workflow");
            string[] split = new string[] { "#;" };
            foreach (KeyValuePair<string, object> kvp in exceptionDict)
            {
                SPListItem workflowItem, eventDataItem;

                GetSpecificWFItem(kvp.Key, out workflowItem, out eventDataItem);

                if (workflowItem != null
                    && eventDataItem != null)
                {
                    try
                    {
                        List<SPWorkflowTask> oldTasks = (kvp.Value as SPWorkflowTask[]).ToList<SPWorkflowTask>();
                        SaveOldTasks(eventDataItem, oldTasks);
                        CancelWorkflowInstance(workflowItem);

                        if (eventDataItem.Fields.ContainsField(Constants.WF_Path_FieldName))
                        {

                            string[] eventDatas = eventDataItem[Constants.WF_Path_FieldName].AsString().Split(split, StringSplitOptions.RemoveEmptyEntries);

                            for (int i = 0; i < eventDatas.Length; i++)
                            {
                                if (i == 0)
                                {
                                    if (StartWorkflowInstance(workflowItem, eventDatas[i]) == null)
                                        break;
                                }
                                else
                                {
                                    if (!ResumeWorkflow(workflowItem, oldTasks, eventDatas[i]))
                                        break;
                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Div);
                        context.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Style, "color:red;");
                        context.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Span);
                        context.HtmlWriter.Write("==>Exception: Trying to unlock {0} failed!", kvp.Key);
                        context.HtmlWriter.RenderEndTag();
                        context.HtmlWriter.RenderEndTag();

                        eventDataItem[SPFieldName.UnlockHistory] = string.Format("Error Message:{0}, Stack Trace:{1}", ex.Message, ex.StackTrace);
                        eventDataItem[SPFieldName.HasException] = 1;
                        eventDataItem.Update();
                    }
                }

            }
        }

        public SPWorkflow StartWorkflowInstance(SPListItem item, string eventData)
        {

            SPWorkflow newWorkflowInstance = null;

            SPFieldUserValue userValue = new SPFieldUserValue(context.Currweb, item[SPBuiltInFieldId.Author].AsString());

            using (SPSite site = new SPSite(context.Currweb.Site.ID, userValue.User.UserToken))
            {
                using (SPWeb web = site.OpenWeb(context.Currweb.ID))
                {
                    SPList list = web.Lists[context.CurrList.Title];
                    SPListItem oitem = list.GetItemById(item.ID);
                    SPWorkflowAssociation wfAss = list.WorkflowAssociations.GetAssociationByName(context.WfName, CultureInfo.CurrentCulture);
                    newWorkflowInstance = site.WorkflowManager.StartWorkflow(oitem, wfAss, eventData);
                }
            }

            return newWorkflowInstance;
        }

        private bool ResumeWorkflow(SPListItem item, List<SPWorkflowTask> oldTasks, string eventDriven)
        {
            bool isSuccessful = false;
            using (SPSite osite = new SPSite(context.Currweb.Site.ID))
            {
                using (SPWeb oweb = osite.OpenWeb(context.Currweb.ID))
                {
                    SPListItem oitem = oweb.Lists[item.ParentList.Title].GetItemById(item.ID);
                    Hashtable hash;

                    SPWorkflow workflow = GetSpecificWorkflow(oitem);

                    foreach (SPWorkflowTask newWorkflowTask in workflow.Tasks)
                    {
                        if (!excutedTaskList.Contains(newWorkflowTask.ID))
                        {
                            if (GetSpecificTaskItem(newWorkflowTask, oldTasks, eventDriven, out hash))
                            {
                                if (SPWorkflowTask.AlterTask(newWorkflowTask as SPListItem, hash, true))
                                {
                                    excutedTaskList.Add(newWorkflowTask.ID);
                                    isSuccessful = true;
                                }
                                break;
                            }
                        }
                    }
                }
            }

            return isSuccessful;
        }
        #endregion

        #region helper method
        private string GetWFKey(int itemId, string workflowListName)
        {
            return string.Format("{0};#{1}", itemId, workflowListName);
        }

        private void SaveOldTasks(SPListItem eventDataItem, List<SPWorkflowTask> oldTasks)
        {
            using (StringWriter stringWriter = new StringWriter())
            {
                using (XmlTextWriter writer = new XmlTextWriter(stringWriter))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Tasks");

                    foreach (SPWorkflowTask task in oldTasks)
                    {
                        writer.WriteStartElement("Task");

                        writer.WriteAttributeString("Title", task.Title);
                        writer.WriteAttributeString("AssignedTo", task[SPBuiltInFieldId.AssignedTo].AsString());
                        writer.WriteAttributeString("Status", task[SPBuiltInFieldId.TaskStatus].AsString());
                        writer.WriteAttributeString("Created", task[SPBuiltInFieldId.Created].AsString());
                        writer.WriteAttributeString("CreatedBy", task[SPBuiltInFieldId.Author].AsString());
                        writer.WriteAttributeString("StartDate", task[SPBuiltInFieldId.StartDate].AsString());
                        writer.WriteAttributeString("CompleteDate", task[SPBuiltInFieldId.Modified].AsString());
                        writer.WriteAttributeString("Outcome", task[SPBuiltInFieldId.WorkflowOutcome].AsString());
                        writer.WriteAttributeString("UIVersion", task[SPBuiltInFieldId._UIVersion].AsString());

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                eventDataItem[SPFieldName.TasksHistory] = stringWriter.ToString();
                eventDataItem.Update();
            }
        }

        private void GetSpecificWFItem(string key, out SPListItem workflowItem, out SPListItem eventDataItem)
        {
            workflowItem = null;
            eventDataItem = null;

            string[] keySplit = new string[] { ";#" };

            string[] keyStrs = key.Split(keySplit, StringSplitOptions.RemoveEmptyEntries);

            int itemId = int.Parse(keyStrs[0]);
            string workflowListName = keyStrs[1];
            Console.WriteLine("Item ID:{0},Workflow List Name:{1}", itemId, workflowListName);

            SPList list = context.Currweb.Lists[workflowListName];

            workflowItem = list.GetItemById(itemId);

            QueryField listNameField = new QueryField("WorkflowListName");
            QueryField idField = new QueryField("Title");

            CAMLExpression<object> exp = idField.Equal(workflowItem.Title) && listNameField.Equal(workflowListName);

            SPListItemCollection eventDataItems = ListQuery.Select()
                                                         .From(context.Currweb.Lists[SPListName.UnlockWorkflow])
                                                         .Where(exp)
                                                         .GetItems();
            Console.WriteLine(eventDataItems.Count);
            if (eventDataItems.Count > 0)
            {
                eventDataItem = eventDataItems[0];
            }

        }

        private void CancelWorkflowInstance(SPListItem item)
        {
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

        private static bool AlterTask(SPListItem task, Hashtable htData, bool fSynchronous, int attempts, int millisecondsTimeout)
        {

            if ((int)task[SPBuiltInFieldId.WorkflowVersion] != 1)
            {
                SPList parentList = task.ParentList.ParentWeb.Lists[new Guid(task[SPBuiltInFieldId.WorkflowListId].ToString())];

                SPListItem parentItem = parentList.Items.GetItemById((int)task[SPBuiltInFieldId.WorkflowItemId]);

                for (int i = 0; i < attempts; i++)
                {
                    SPWorkflow workflow = parentItem.Workflows[new Guid(task[SPBuiltInFieldId.WorkflowInstanceID].ToString())];

                    if (!workflow.IsLocked)
                    {

                        task[SPBuiltInFieldId.WorkflowVersion] = 1;

                        task.SystemUpdate();

                        break;

                    }

                    if (i != attempts - 1)

                        Thread.Sleep(millisecondsTimeout);

                }

            }

            return SPWorkflowTask.AlterTask(task, htData, fSynchronous);

        }

        private SPWorkflow GetSpecificWorkflow(SPListItem item)
        {
            SPWorkflow returnWorkflow = null;
            foreach (SPWorkflow workflow in item.Web.Site.WorkflowManager.GetItemActiveWorkflows(item))
            {
                if (item.ParentList.WorkflowAssociations[workflow.AssociationId].Name.Equals(context.WfName, StringComparison.CurrentCultureIgnoreCase))
                {
                    returnWorkflow = workflow;
                }
            }
            return returnWorkflow;
        }

        private bool GetSpecificTaskItem(SPWorkflowTask newWorkflowTask, List<SPWorkflowTask> oldTasks, string eventDriven, out Hashtable hash)
        {
            bool isSpecific = false;
            hash = new Hashtable();

            SPFieldUserValue newTaskUserValue = new SPFieldUserValue(newWorkflowTask.Web, newWorkflowTask[SPBuiltInFieldId.AssignedTo].AsString());

            foreach (SPWorkflowTask oldTask in oldTasks)
            {
                SPFieldUserValue oldTaskUserValue = new SPFieldUserValue(newWorkflowTask.Web, oldTask[SPBuiltInFieldId.AssignedTo].AsString());
                if (newTaskUserValue.User.ID.Equals(oldTaskUserValue.User.ID))
                {
                    string outCome = oldTask[SPBuiltInFieldId.WorkflowOutcome].AsString();
                    hash.Add("__TaskOutcome", outCome);
                    hash.Add("__Action", "Commit");
                    hash.Add("__WorkflowVaribales", eventDriven);
                    //hash.Add("AssignedTo", oldTaskUserValue.User);

                    isSpecific = true;

                    oldTasks.Remove(oldTask);
                    break;
                }
            }

            return isSpecific;
        }

        private bool IsErrorOccured(SPWorkflowState state)
        {
            bool isError = false;
            if ((state & (SPWorkflowState.Running | SPWorkflowState.Faulting))
                        == (SPWorkflowState.Running | SPWorkflowState.Faulting))
            {
                isError = true;
            }

            return isError;
        }

        private bool IsErrorOccured(string status)
        {
            List<string> errorOccuredList = new List<string>() { "3", "7", "6", "1" };
            bool isError = false;
            if (errorOccuredList.Contains(status))
            {
                isError = true;
            }

            return isError;
        }

        private void AddExceptionList(string key, SPWorkflowTaskCollection tasks)
        {

            SPWorkflowTask[] taskArray = new SPWorkflowTask[tasks.Count];
            tasks.CopyTo(taskArray, 0);

            if (!exceptionDict.ContainsKey(key))
            {
                exceptionDict.Add(key, taskArray);
            }
        }

        #endregion

        public override void RetryingWF(int itemId)
        {
            throw new NotImplementedException();
        }

        public override void ReassignWorkflowTask(int itemId, int taskItemId, int reassignUserId)
        {
            throw new NotImplementedException();
        }
    }
}
