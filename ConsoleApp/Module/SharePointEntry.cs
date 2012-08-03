using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using ConsoleApp.Constants;
using ConsoleApp.Extensions;
using Microsoft.SharePoint.Workflow;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.Office.Server;
using System.Collections;
using System.Globalization;
using System.Net;
using System.Configuration;
using System.Xml;
using SAP.Middleware;
using SAP.Middleware.Exchange;
using SAP.Middleware.Table;
using System.Data;
using ConsoleApp.Services;
using ConsoleApp.Interface;
using System.Threading;
using System.Collections.Specialized;
using System.Web;
using System.Text.RegularExpressions;
using Microsoft.SharePoint.Utilities;

namespace ConsoleApp.Module
{
    class SharePointEntry
    {

        internal static void GetSPFieldUserValue(SPWeb web)
        {
            SPList list = web.Lists[SPListName.TravelExpenseClaim];

            foreach (SPListItem item in list.Items)
            {
                string ss = string.Empty;
                SPUser user = web.EnsureUser(item["ApplicantSPUser"].ToString().Split(new string[] { ";#" }, StringSplitOptions.None)[1]);
                Console.WriteLine(item["ApplicantSPUser"].ToString());
            }
        }

        internal static void UpdateSPListItemLinkValue(SPWeb web)
        {
            SPList list = web.Lists["Credit Card Claim"];

            SPFieldUrlValue link = new SPFieldUrlValue();
            link.Description = "Closed";
            link.Url = "http://www.baidu.com";
            foreach (SPListItem item in list.Items)
            {
                if (item.ID == 3728)
                {
                    item["Link"] = link;
                    item.Update();
                }
            }
        }

        internal static void TerminateWorkflowTask(SPWeb web)
        {
            web.AllowUnsafeUpdates = true;
            SPList list = web.Lists[SPListName.CreditCardClaim];
            SPWorkflowAssociation wfAss = list.WorkflowAssociations.GetAssociationByName(WorkflowName.CreditCardClaim, System.Globalization.CultureInfo.CurrentCulture);

            SPListItem item = list.GetItemById(13);

            foreach (SPWorkflow wfItem in item.Workflows)
            {
                SPWorkflowManager.CancelWorkflow(wfItem);
            }

            web.AllowUnsafeUpdates = false;
        }

        internal static void FindLockedWorkflow(SPWeb web)
        {
            List<string> PRLockedWorkflowList = new List<string>();
            List<string> PRExceptionList = new List<string>();

            string templateFormat = @"<html><body>{0}</body></html>";

            StringBuilder contentBuilder = new StringBuilder();
            string contentFormat = @"{0}<br/>";


            foreach (SPList list in web.Lists)
            {
                if (list.Title == SPListName.PurchaseRequest)
                {
                    foreach (SPListItem item in list.Items)
                    {
                        string workflowID = item[SPBuiltInFieldId.Title].AsString();

                        //Console.WriteLine(string.Format("Checking item {0} ....", workflowID));
                        foreach (SPWorkflow workflow in item.Workflows)
                        {
                            //SystemEntry.LogInfoToLogFile("workflow info",
                            //                            string.Format(@"{0}:{1} Workflow Name:{2} InnerState:{3}",
                            //                                item.ParentList.Title,
                            //                                workflowID,
                            //                                item.ParentList.WorkflowAssociations[workflow.AssociationId].Name,
                            //                                workflow.InternalState.ToString()
                            //                                ),
                            //                                "FindLockedWorkflow");
                            if (workflow.IsLocked)
                            {
                                PRLockedWorkflowList.Add(workflowID);
                                Console.WriteLine(string.Format("{0}:{1} is locked!", item.ParentList.Title, workflowID));
                                contentBuilder.AppendFormat(contentFormat, string.Format("{0}:{1} is locked!", item.ParentList.Title, workflowID));
                            }

                            if (workflow.InternalState.ToString().Contains("Error"))
                            {
                                if (!PRLockedWorkflowList.Contains(workflowID)
                                    && !PRExceptionList.Contains(workflowID))
                                {
                                    PRExceptionList.Add(workflowID);
                                }
                                Console.WriteLine(string.Format("[According to the Workflow Internal State] Exceptions occured on {0}:{1}!", item.ParentList.Title, workflowID));
                                contentBuilder.AppendFormat(contentFormat, string.Format("[According to the Workflow Internal State] Exceptions occured on {0}:{1}!", item.ParentList.Title, workflowID));
                            }

                            #region According to the WorkflowVersion
                            foreach (SPWorkflowTask task in workflow.Tasks)
                            {
                                if (task[SPBuiltInFieldId.WorkflowVersion].AsString() != "1")
                                {
                                    if (!PRLockedWorkflowList.Contains(workflowID))
                                    {
                                        PRLockedWorkflowList.Add(workflowID);
                                    }
                                    Console.WriteLine(string.Format("[According to the WorkflowVersion] {0}:{1} is locked!",
                                                            item.ParentList.Title,
                                                            workflowID));
                                    contentBuilder.AppendFormat(contentFormat, string.Format("[According to the WorkflowVersion] {0}:{1} is locked!",
                                                            item.ParentList.Title,
                                                            workflowID));
                                    break;
                                }
                            }
                            #endregion

                            #region According to the Workflow History
                            foreach (SPListItem workflowHistoryItem in workflow.HistoryList.Items)
                            {

                                if (string.Format(@"{{{0}}}", workflow.InstanceId.ToString()).Equals(workflowHistoryItem["Workflow History Parent Instance"].AsString(), StringComparison.CurrentCultureIgnoreCase)
                                    && item.ID.Equals(workflowHistoryItem["Primary Item ID"]))
                                {

                                    if (workflowHistoryItem[SPBuiltInFieldId.Outcome].AsString().Contains("Transaction"))
                                    {
                                        if (!PRLockedWorkflowList.Contains(workflowID)
                                             && !PRExceptionList.Contains(workflowID))
                                        {
                                            PRExceptionList.Add(workflowID);
                                        }
                                        SystemEntry.LogInfoToLogFile("workflow history info",
                                                              string.Format("[According to the Workflow History] Exceptions occured on {0}:{1}!\nOutcome:{2}",
                                                                              item.ParentList.Title,
                                                                              workflowID,
                                                                              workflowHistoryItem[SPBuiltInFieldId.Outcome].AsString()),
                                                               "FindLockedWorkflow");

                                        Console.WriteLine(string.Format("[According to the Workflow History] Exceptions took place on {0}:{1}!",
                                                            item.ParentList.Title,
                                                            workflowID));
                                        contentBuilder.AppendFormat(contentFormat, string.Format("[According to the Workflow History] Exceptions took place on {0}:{1}!",
                                                            item.ParentList.Title,
                                                            workflowID));
                                        break;
                                    }
                                }
                            }
                            #endregion

                        }
                    }


                    if (PRLockedWorkflowList.Count > 0)
                    {
                        Console.WriteLine(string.Format("[Summary] There are {0} PR Items locked!\n{1}",
                                           PRLockedWorkflowList.Count.ToString(),
                                           string.Join(", ", PRLockedWorkflowList.ToArray())));
                        contentBuilder.AppendFormat(contentFormat, string.Format("[Summary] There are {0} PR Items locked!\n{1}",
                                           PRLockedWorkflowList.Count.ToString(),
                                           string.Join(", ", PRLockedWorkflowList.ToArray())));
                    }

                    if (PRExceptionList.Count > 0)
                    {
                        Console.WriteLine(string.Format("[Summary] There are {0} PR Items that has exceptions!\n{1}",
                                           PRExceptionList.Count.ToString(),
                                           string.Join(", ", PRExceptionList.ToArray())));
                        contentBuilder.AppendFormat(contentFormat, string.Format("[Summary] There are {0} PR Items that has exceptions!\n{1}",
                                           PRExceptionList.Count.ToString(),
                                           string.Join(", ", PRExceptionList.ToArray())));
                    }

                }
            }

            if (contentBuilder.ToString().IsNotNullOrWhitespace())
            {
                SystemEntry.LogInfoToLogFile("Send Mail Started!", "Send Mail", "SendMail");
                string body = string.Format(templateFormat, contentBuilder.ToString());

                //ThreadPool.QueueUserWorkItem(delegate
                //{

                IMailService mailService = MailServiceFactory.GetMailService();
                StringCollection recipients = new StringCollection();
                recipients.Add("wang_jingchao@vanceinfo.com");
                recipients.Add("huang_zhigang@vanceinfo.com");

                try
                {
                    mailService.SendMail(string.Format("About Purchase Request Locked Workflow({0} Items)", PRLockedWorkflowList.Count + PRExceptionList.Count), body, recipients);
                }
                catch (Exception ex)
                {
                    SystemEntry.LogInfoToLogFile("Send Mail Failed!", string.Format("Error Message:{0}\nStack Trace:{1}\nSource Info:{2}", ex.Message, ex.StackTrace, ex.Source), "SendMail");
                }

                //});


            }
        }

        internal static void UnLockWorkflowTasks(SPWeb web)
        {
            Console.WriteLine("Please input the workflow list name:");
            string wfListName = SPListName.EmployeeExpenseClaim;
            Console.WriteLine(wfListName);
            Console.WriteLine("Please input the workflow Id:");
            int itemId = 549;
            Console.WriteLine(itemId);

            SPList list = web.Lists[wfListName];
            if (list != null)
            {
                SPListItem wfItem = list.GetItemById(itemId);
                if (wfItem != null)
                {
                    UnLockWorkflowTasks(wfItem);
                }
                else
                {
                    Console.WriteLine(string.Format("Sorry, the workflow number({0}) does not exist, please resubmit it.", itemId));
                }
            }
            else
            {
                Console.WriteLine(string.Format("Sorry, the list({1}) does not exist, please resubmit it.", wfListName));
            }
        }

        private static void UnLockWorkflowTasks(SPListItem wfItem)
        {
            bool isNeedUnlock = false;
            foreach (SPWorkflow workflow in wfItem.Workflows)
            {
                Console.WriteLine(wfItem.ParentList.WorkflowAssociations[workflow.AssociationId].Name);
                foreach (SPWorkflowTask task in workflow.Tasks)
                {
                    if (task[SPBuiltInFieldId.WorkflowVersion].AsString() != "1")
                    {
                        isNeedUnlock = true;

                        task[SPBuiltInFieldId.WorkflowVersion] = "1";
                        task.SystemUpdate();
                    }
                }
            }

            if (isNeedUnlock)
            {
                Console.WriteLine(string.Format("The workflow {0} is unlocked now.", wfItem["Title"].AsString()));
            }
            else
            {
                Console.WriteLine(string.Format("The workflow {0} was not locked before.", wfItem["Title"].AsString()));
            }
        }

        private static SPListItem GetWFItem(SPList list, string wfNo)
        {
            SPListItem wfItem = null;

            foreach (SPListItem item in list.Items)
            {
                if (item["Title"].AsString().Equals(wfNo, StringComparison.CurrentCultureIgnoreCase))
                {
                    wfItem = item;
                    break;
                }
            }

            return wfItem;
        }

        internal static void GetCEOName(SPWeb web)
        {
            SPGroup group = web.Groups["wf_CEO"];
            foreach (SPUser user in group.Users)
            {
                Console.WriteLine(string.Format("Login Name:{0}", user.LoginName));
            }
        }

        //internal static void GetUserProfile(SPWeb web)
        //{
        //    string userAccount = "CNAIDC\\mark.siezen";
        //    try
        //    {
        //        //从SSP里面取得当前用户所在的部门名称
        //        SPSecurity.RunWithElevatedPrivileges(delegate()
        //        {
        //            using (SPSite site = new SPSite(web.Site.ID))
        //            {
        //                ServerContext context = ServerContext.GetContext(site);
        //                UserProfileManager profileManager = new UserProfileManager(context);

        //                if (profileManager.UserExists(userAccount) || SPContext.Current.Web.CurrentUser.IsSiteAdmin)
        //                {
        //                    if (SPContext.Current.Web.CurrentUser.IsSiteAdmin && userAccount == SPContext.Current.Web.CurrentUser.LoginName)
        //                    {
        //                        userAccount = System.Web.HttpContext.Current.User.Identity.Name;
        //                    }
        //                    UserProfile userProfile = profileManager.GetUserProfile(userAccount);

        //                }
        //                else
        //                {
        //                    //throw new Exception("共享服务(SSP)中没有该用户的信息，可能是共享服务(SSP)没有和AD同步所致，请联系IT管理员联系！");
        //                }
        //            }
        //        }
        //        );
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("获取当前的用户信息出错：" + e.Message);
        //    }

        //    return employee;
        //}

        internal static void GetTaskListItemInfo(SPWeb web)
        {
            SPList list = web.Lists[SPListName.Tasks];
            foreach (SPListItem item in list.Items)
            {
                if (item[SPBuiltInFieldId.WorkflowItemId].AsString() == "1")
                {
                    Console.WriteLine("Task Title:{0}\nTask Item Id:{1}\nTask Association List Id:{2}\nTask WorkflowName{3}",
                        item.Title,
                        item.ID,
                        item[SPBuiltInFieldId.WorkflowListId].AsString(),
                        item[SPBuiltInFieldId.WorkflowName].AsString());
                }
            }
        }

        internal static void InsertListItemToTask(SPWeb web)
        {
            SPList list = web.Lists[SPListName.Tasks];


            //            SPQuery query = new SPQuery();
            //            query.Query = @"<Where>
            //                              <Eq>
            //                                 <FieldRef Name='Title' />
            //                                 <Value Type='Text'>PR000337</Value>
            //                              </Eq>
            //                           </Where>";
            //            SPListItemCollection items = web.Lists[SPListName.PurchaseRequest].GetItems(query);

            //if (items.Count > 0)
            int count = 500000;
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine(i);
                SPListItem item = list.Items.Add();
                item["Title"] = "test";
                //Hashtable hash = new Hashtable();

                //hash.Add(SPBuiltInFieldId.Title, "Shining Xu's Purchase Request  \"PR000107\" needs approval");
                //hash.Add(SPBuiltInFieldId.AssignedTo, "21");
                ////hash.Add(SPBuiltInFieldId._Status, "Completed");
                //hash.Add(SPBuiltInFieldId.WorkflowLink, "https://portal.c-and-a.cn/WorkFlowCenter/Lists/PurchaseRequestWorkflow/DispForm.aspx?ID=105, PR000107");
                //hash.Add(SPBuiltInFieldId.StartDate, "2011/12/13 12:34:53");
                ////hash.Add(SPBuiltInFieldId.DateCompleted, "2011/12/13 13:22:56");
                ////hash.Add(SPBuiltInFieldId.Last_x0020_Modified, "2011/12/13 13:22:56");
                //hash.Add(SPBuiltInFieldId.WorkflowListId, "a4a80816-43fe-4621-8ab8-8bdd0e406ae6");
                //hash.Add(SPBuiltInFieldId.WorkflowItemId, "176");
                //hash.Add(SPBuiltInFieldId.WorkflowOutcome, "Approve");

                //item.Fields["Status"].ReadOnlyField = false;
                //item.Fields["Status"].Update();
                //item["Status"] = "Completed";

                //item.Fields["Modified"].ReadOnlyField = false;
                //item.Fields["Modified"].Update();
                //item["Modified"] = "2011/12/13 13:22:56";

                //List<SPField> readonlyFieldList = new List<SPField>();
                ////foreach (SPField field in items[0].Fields)
                ////{
                ////    if (item.Fields.ContainsField(field.StaticName))
                ////    {
                ////        if (field.ReadOnlyField)
                ////        {
                ////            readonlyFieldList.Add(field);
                ////            field.ReadOnlyField = false;
                ////            field.Update();
                ////        }

                ////        try
                ////        {
                ////            item[field.StaticName] = items[0][field.StaticName].AsString();
                ////        }
                ////        catch (Exception e)
                ////        {
                ////            Console.WriteLine(string.Format("Can't update the field {0}.", field.StaticName));
                ////            continue;
                ////        }
                ////    }

                ////}

                //SPField itemField;
                //foreach (DictionaryEntry entry in hash)
                //{
                //    itemField = item.Fields[(Guid)entry.Key];
                //    if (itemField.ReadOnlyField)
                //    {
                //        itemField.ReadOnlyField = false;
                //        itemField.Update();
                //        readonlyFieldList.Add(itemField);
                //    }
                //    item[(Guid)entry.Key] = entry.Value.ToString();

                //}

                item.SystemUpdate();

                //foreach (SPField field in readonlyFieldList)
                //{
                //    field.ReadOnlyField = true;
                //    field.Update();
                //}
            }

            //else
            //{
            //    Console.WriteLine(string.Format("Could not find {0} in the list.", workflowNumber));
            //}
        }

        internal static void FindListByTemplateFeatureName(SPWeb web)
        {
            string featureName = "CreditCardClaimWorkFlow";

            Guid templateFeatureId = new Guid();
            List<string> templateListNames = new List<string>();

            try
            {
                foreach (SPFeature feature in web.Features)
                {
                    if (feature.Definition.DisplayName.Equals(featureName))
                    {
                        templateFeatureId = feature.DefinitionId;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Can't find feature according to the feature name,details:{0}", e.Message));
            }

            Console.WriteLine(string.Format("According to the Feature Name {0}, the following lists are found, are you sure to delete it?(Sure/No)",
                                            featureName));

            foreach (SPList list in web.Lists)
            {
                if (list.TemplateFeatureId.Equals(templateFeatureId))
                {
                    Console.WriteLine(list.Title);
                    if (!templateListNames.Contains(list.Title, new MyComparer.MyCaseInsensitiveComparer()))
                    {
                        templateListNames.Add(list.Title);
                    }
                }
            }

            string isContinue = Console.ReadLine();
            if (isContinue.Equals("sure", StringComparison.CurrentCultureIgnoreCase))
            {
                foreach (string listName in templateListNames)
                {
                    web.Lists[listName].Delete();
                    web.Update();
                }

                Console.WriteLine("Successful!");

            }

        }

        internal static void SaveListTemplateByFeatureName(SPWeb web)
        {
            string featureName = "CreditCardClaimWorkFlow";

            List<SPList> templateLists = new List<SPList>();
            Guid templateFeatureId = new Guid();

            foreach (SPFeature feature in web.Features)
            {
                if (feature.Definition.DisplayName.Equals(featureName))
                {
                    templateFeatureId = feature.DefinitionId;
                    break;
                }
            }

            foreach (SPList list in web.Lists)
            {
                if (list.TemplateFeatureId.Equals(templateFeatureId))
                {
                    templateLists.Add(list);
                }
            }

            foreach (SPList list in templateLists)
            {
                list.SaveAsTemplate(string.Format("{0}.stp", list.Title), list.Title, string.Empty, false);
            }

        }

        internal static void UpdateReadOnlyField(SPWeb web)
        {
            string listName = SPListName.Tasks;
            List<int> idList = new List<int>() { 13578 };
            SPList list = web.Lists[listName];
            List<SPField> readonlyFieldList = new List<SPField>();

            if (list != null)
            {
                foreach (int id in idList)
                {
                    DataTable dt = list.Items.GetDataTable();
                    SPListItem item = list.GetItemById(id);
                    SPField field = item.Fields["Modified"];
                    if (field.ReadOnlyField)
                    {
                        readonlyFieldList.Add(field);
                        field.ReadOnlyField = false;
                        field.Update();
                    }

                    switch (id)
                    {
                        case 16740:
                            item["Outcome"] = "Confirm";
                            break;
                        default:
                            item["Modified"] = "2011-03-22 13:22:56";
                            break;
                    }

                    //Hashtable hash = SPWorkflowTask.GetExtendedPropertiesAsHashtable(item);
                    //SPWorkflowTask.AlterTask(item, hash, true);
                    //item[SPBuiltInFieldId.WorkflowVersion] = 1;
                    //item["ows_WorkflowVaribales"] = @"<?xml version='1.0' encoding='utf-8'?><Data><item type='QuickFlow.NameCollection, QuickFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ec1e0fe6e1745628'><key><string>ConfirmTaskUsers</string></key><value><ArrayOfString xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><string>CNAIDC\Shining.Xu</string></ArrayOfString></value></item><item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>IsContinue</string></key><value><boolean>false</boolean></value></item></Data>";
                    item.Update();

                    foreach (SPField spfield in readonlyFieldList)
                    {
                        spfield.ReadOnlyField = true;
                        spfield.Update();
                    }

                }

                Console.WriteLine("Successful!");
            }

        }

        internal static void GetWorkflowInfo(SPWeb web)
        {
            SPList list = web.Lists[SPListName.CashAdvanceRequest];
            //List<int> idList = new List<int>() { 60 };
            //            foreach (int id in idList)
            //            {
            //                SPListItem wfItem = list.GetItemById(id);
            //                foreach (SPWorkflow workflow in wfItem.Workflows)
            //                {
            //                    Console.WriteLine(wfItem.ParentList.WorkflowAssociations[workflow.AssociationId].Name);
            //                    SystemEntry.LogInfoToLogFile("Task XML", workflow.Tasks.Xml, "GetWorkflowInfo");
            //                    Console.WriteLine(string.Format(@"WF State:{0}\n,
            //                                        WF History List:{1}\n,
            //                                        WF Info:{2}.", workflow.InternalState, workflow.HistoryList, workflow.AssociationId));
            //                }
            //            }
            foreach (SPWorkflowAssociation association in list.WorkflowAssociations)
            {
                Console.WriteLine(association.Id);
            }
            foreach (SPWorkflow workflow in list.Items[0].Workflows)
            {

                Console.WriteLine("{0}[{1}]", workflow.AssociationId, list.WorkflowAssociations[workflow.AssociationId].Name);
            }
        }

        internal static void GetTaskField(SPWeb web)
        {
            SPList list = web.Lists[SPListName.Tasks];
            SPField field = list.Items[0].Fields[SPBuiltInFieldId._UIVersion];

        }

        internal static void StartWorkflowInstance(SPWeb web)
        {
            //StartWorkflowInstanceForNEEA(web);
            StartWorkflowInstanceForPR(web);

        }

        private static void StartWorkflowInstanceForNEEA(SPWeb web)
        {
            #region EventDataFormat
            string eventDataFormat = @"<?xml version='1.0' encoding='utf-8'?>
                                        <Data>{0}</Data>";

            string NameCollectionFormat = @"<item type='QuickFlow.NameCollection, QuickFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ec1e0fe6e1745628'>
            		                            <key><string>{0}</string></key>
            		                            <value>
            			                            <ArrayOfString xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
            				                            <string>{1}</string>
            			                            </ArrayOfString>
            		                            </value>        		                           
            	                            </item>";

            string BooleanFormat = @"<item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'>
			                                <key><string>{0}</string></key>
		                                    <value><boolean>{1}</boolean></value>
	                                </item>";

            string StringFormat = @"<item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'>
 			                            <key><string>{0}</string></key>
			                            <value><string>{1}</string></value>
		                            </item>";
            #endregion

            #region Parameters
            string listName = SPListName.NewEmployeeEquipmentApplication;
            string wfName = WorkflowName.NewEmployeeEquipmentApplication;

            string wfId = @"NEEA_000125";
            string applicantAccount = @"CNAIDC\pamela.hou";
            string applicantDisplayName = @"Pamela Hou";
            string approverAccount = @"CNAIDC\james.lee2";

            List<string> ITGroupTaskUsers = new List<string>();
            ITGroupTaskUsers.Add(@"CNAIDC\jimmy.wang");
            ITGroupTaskUsers.Add(@"CNAIDC\helpdesk");
            ITGroupTaskUsers.Add(@"CNAIDC\sara.sun");
            string ITGroupTaskUsersStr = string.Join(@"</string><string>", ITGroupTaskUsers.ToArray());

            string taskTitle = applicantDisplayName + "'s Equipment Application";

            string HRSubmitTitle = "Please complete equipment application";
            string FunctionalManagerApproveTitle = taskTitle + " needs approval";
            string DepartmentHeadApproveTitle = taskTitle + " needs approval";
            string ITGroupConfirmTitle = taskTitle + " needs confirm";

            string vs_HRSubmitTitle = string.Format(StringFormat, "HRSubmitTitle", HRSubmitTitle);
            string vs_FunctionalManagerApproveTitle = string.Format(StringFormat, "FunctionalManagerApproveTitle", FunctionalManagerApproveTitle);
            string vs_DepartmentHeadApproveTitle = string.Format(StringFormat, "DepartmentHeadApproveTitle", DepartmentHeadApproveTitle);
            string vs_ITGroupConfirmTitle = string.Format(StringFormat, "ITGroupConfirmTitle", ITGroupConfirmTitle);

            //Console.WriteLine(ITGroupTaskUsersStr);
            //string vs_ITGroup = string.Format(NameCollectionFormat, "ITGroup", ITGroupTaskUsersStr);
            string vs_ITGroup = string.Format(StringFormat, "ITGroup", "wf_IT");
            string vs_DH = string.Format(StringFormat, "DH", approverAccount);
            string vs_FunctionalManager = string.Format(StringFormat, "FunctionalManager", approverAccount);

            string eventData = string.Format(eventDataFormat, vs_HRSubmitTitle
                                                              + vs_FunctionalManagerApproveTitle
                                                              + vs_DepartmentHeadApproveTitle
                                                              + vs_ITGroupConfirmTitle
                                                              + vs_ITGroup
                                                              + vs_DH
                                                              + vs_FunctionalManager);
            SystemEntry.LogInfoToLogFile("Event Data", eventData, "StartWorkflowInstanceForNEEA");
            #endregion

            SPUser user = web.AllUsers[applicantAccount];
            SPList list = web.Lists[listName];

            SPQuery query = new SPQuery();
            string queryFormat = @"<Where>
                                    <Eq>
                                        <FieldRef Name='Title' />
                                        <Value Type='Text'>{0}</Value>
                                    </Eq>
                                </Where>";
            query.Query = string.Format(queryFormat, wfId);

            SPListItemCollection items = list.GetItems(query);
            if (items.Count > 0)
            {
                SPListItem item = list.Items.GetItemById(items[0].ID);

                using (SPSite osite = new SPSite(web.Site.ID, user.UserToken))
                {
                    using (SPWeb oweb = osite.OpenWeb(web.ID))
                    {

                        SPWorkflowAssociation wfAss = list.WorkflowAssociations.GetAssociationByName(wfName, CultureInfo.InvariantCulture);

                        osite.WorkflowManager.StartWorkflow(item, wfAss, eventData);
                    }
                }
            }
        }

        private static void StartWorkflowInstanceForPR(SPWeb web)
        {
            #region EventDataFormat
            string eventDataFormat = @"<?xml version='1.0' encoding='utf-8'?>
                                        <Data>{0}</Data>";

            string NameCollectionFormat = @"<item type='QuickFlow.NameCollection, QuickFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ec1e0fe6e1745628'>
            		                            <key><string>{0}</string></key>
            		                            <value>
            			                            <ArrayOfString xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
            				                            <string>{1}</string>
            			                            </ArrayOfString>
            		                            </value>        		                           
            	                            </item>";

            string BooleanFormat = @"<item type='System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'>
			                                <key><string>{0}</string></key>
		                                    <value><boolean>{1}</boolean></value>
	                                </item>";

            string StringFormat = @"<item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'>
 			                            <key><string>{0}</string></key>
			                            <value><string>{1}</string></value>
		                            </item>";
            #endregion

            #region Parameters
            string listName = SPListName.PurchaseRequest;
            string wfName = WorkflowName.PurchaseRequest;

            string wfId = @"PR000971";
            SPUser applicantSPUser = GetApplicantSPUser(web.Lists[listName], wfId);
            string applicantAccount = applicantSPUser.LoginName;
            string applicantDisplayName = applicantSPUser.Name;

            Console.WriteLine("User Account:{0},Display Name:{1}", applicantAccount, applicantDisplayName);
            //Console.Read();

            string taskTitle = applicantDisplayName + "'s Purchase Request";

            string CompleteTaskTitle = "Please complete equipment application";
            string CheckTaskTitle = taskTitle + " needs approval";
            string ApproveTaskTitle = taskTitle + " needs approval";
            string ConfirmTaskTitle = taskTitle + " needs confirm";

            string vs_CompleteTaskTitle = string.Format(StringFormat, "CompleteTaskTitle", CompleteTaskTitle);
            string vs_CheckTaskTitle = string.Format(StringFormat, "CheckTaskTitle", CheckTaskTitle);
            string vs_ApproveTaskTitle = string.Format(StringFormat, "ApproveTaskTitle", ApproveTaskTitle);
            string vs_ConfirmTaskTitle = string.Format(StringFormat, "ConfirmTaskTitle", ConfirmTaskTitle);

            string editURL = "/_Layouts/CA/WorkFlows/PurchaseRequest/EditForm.aspx";
            string approveURL = "/_Layouts/CA/WorkFlows/PurchaseRequest/ApproveForm.aspx";
            string checkURL = "/_Layouts/CA/WorkFlows/PurchaseRequest/CheckForm.aspx";
            string confirmURL = "/_Layouts/CA/WorkFlows/PurchaseRequest/ConfirmForm.aspx";

            string vs_CompleteTaskFormURL = string.Format(StringFormat, "CompleteTaskFormURL", editURL);
            string vs_CheckTaskFormURL = string.Format(StringFormat, "CheckTaskFormURL", checkURL);
            string vs_ApproveTaskFormURL = string.Format(StringFormat, "ApproveTaskFormURL", approveURL);
            string vs_ConfirmTaskFormURL = string.Format(StringFormat, "ConfirmTaskFormURL", confirmURL);

            string vs_IsSave = string.Format(BooleanFormat, "IsSave", "false");
            string vs_IsSkipApprove = string.Format(BooleanFormat, "IsSkipApprove", "false");
            string vs_IsHO = string.Format(BooleanFormat, "IsHO", "false");

            //Console.WriteLine(ITGroupTaskUsersStr);
            //string vs_ITGroup = string.Format(NameCollectionFormat, "ITGroup", ITGroupTaskUsersStr);

            List<string> CheckTaskUsers = new List<string>();
            //CheckTaskUsers.Add(web.SiteUsers.GetByID(476).LoginName);
            CheckTaskUsers.Add(web.SiteUsers.GetByID(503).LoginName);
            CheckTaskUsers.Add(web.SiteUsers.GetByID(581).LoginName);
            string CheckTaskUsersStr = string.Join(@"</string><string>", CheckTaskUsers.ToArray());

            //string vs_ApproveTaskUsers = string.Format(StringFormat, "ApproveTaskUsers", approverAccount);
            string vs_CheckTaskUsers = string.Format(NameCollectionFormat, "CheckTaskUsers", CheckTaskUsersStr);
            //string vs_ConfirmTaskUsers = string.Format(StringFormat, "ConfirmTaskUsers", approverAccount);

            List<string> ApproveTaskUsers = new List<string>();
            ApproveTaskUsers.Add(web.SiteUsers.GetByID(461).LoginName);
            //ApproveTaskUsers.Add(@"CNAIDC\Debbie.Yang");
            string ApproveTaskUsersStr = string.Join(@"</string><string>", ApproveTaskUsers.ToArray());

            //string vs_ApproveTaskUsers = string.Format(StringFormat, "ApproveTaskUsers", approverAccount);
            string vs_ApproveTaskUsers = string.Format(NameCollectionFormat, "ApproveTaskUsers", ApproveTaskUsersStr);

            List<string> wfVariables = new List<string>();

            wfVariables.Add(vs_IsHO);
            wfVariables.Add(vs_IsSave);
            wfVariables.Add(vs_IsSkipApprove);

            wfVariables.Add(vs_ConfirmTaskTitle);
            wfVariables.Add(vs_ConfirmTaskFormURL);

            wfVariables.Add(vs_CompleteTaskTitle);
            wfVariables.Add(vs_CompleteTaskFormURL);

            wfVariables.Add(vs_CheckTaskTitle);
            wfVariables.Add(vs_CheckTaskFormURL);

            wfVariables.Add(vs_ApproveTaskTitle);
            wfVariables.Add(vs_ApproveTaskFormURL);

            wfVariables.Add(vs_CheckTaskUsers);
            //wfVariables.Add(vs_ConfirmTaskUsers);
            //wfVariables.Add(vs_ApproveTaskUsers);

            string wfVariablesStr = string.Empty;
            foreach (string s in wfVariables)
            {
                wfVariablesStr += s;
            }
            string eventData = string.Format(eventDataFormat, wfVariablesStr);

            SystemEntry.LogInfoToLogFile("EventDataInfo", eventData, "StartWorkflowInstanceForPR");
            #endregion

            SPUser user = web.AllUsers[applicantAccount];
            SPList list = web.Lists[listName];

            SPQuery query = new SPQuery();
            string queryFormat = @"<Where>
                                        <Eq>
                                            <FieldRef Name='Title' />
                                            <Value Type='Text'>{0}</Value>
                                        </Eq>
                                    </Where>";
            query.Query = string.Format(queryFormat, wfId);

            SPListItemCollection items = list.GetItems(query);
            if (items.Count > 0)
            {
                SPListItem item = list.Items.GetItemById(items[0].ID);

                using (SPSite osite = new SPSite(web.Site.ID, user.UserToken))
                {
                    using (SPWeb oweb = osite.OpenWeb(web.ID))
                    {
                        SPList olist = oweb.Lists[list.Title];
                        SPListItem oitem = olist.Items.GetItemById(items[0].ID);
                        SPWorkflowAssociation wfAss = olist.WorkflowAssociations.GetAssociationByName(wfName, CultureInfo.InvariantCulture);

                        osite.WorkflowManager.StartWorkflow(oitem, wfAss, eventData);
                    }
                }

                //Console.WriteLine("Is Continue?");
                //string input = Console.ReadLine();
                //if (input.Equals("Y", StringComparison.CurrentCultureIgnoreCase))
                //{
                //    item["CheckPerson"] = @"CNAIDC\Shining.Xu";
                //    SPUser spUser = web.AllUsers[@"CNAIDC\spsadmin"];
                //    SPFieldUserValue userValue = new SPFieldUserValue(web, spUser.ID, spUser.LoginName);
                //    SPFieldUserValueCollection userCollection = item.Fields["ApproversSP"].GetFieldValue(item["ApproversSP"].AsString()) as SPFieldUserValueCollection;
                //    userCollection.Remove(userValue);
                //    item["ApproversSP"] = userCollection;
                //    item.SystemUpdate();
                //}
            }


        }

        private static SPUser GetApplicantSPUser(SPList list, string wfId)
        {
            SPUser user = null;
            SPListItemCollection items = GetListItemsByQuery(list, @"<Where>
                                                                    <Eq>
                                                                        <FieldRef Name='Title' />
                                                                        <Value Type='Text'>{0}</Value>
                                                                    </Eq>
                                                                </Where>", wfId);
            if (items.Count > 0)
            {
                Regex r = new Regex(@"^.*\((.*)\)$");
                string loginName = r.Match(items[0]["Applicant"].AsString()).Groups[1].ToString();
                user = list.ParentWeb.AllUsers[loginName];
            }

            return user;
        }

        internal static void ExamDiffFieldsByListService(SPWeb web)
        {
            List<string> listNames = new List<string>();
            listNames.Add(SPListName.TravelRequestWorkflow2);
            listNames.Add(SPListName.TravelHotelInfo2);
            listNames.Add(SPListName.TravelVehicleInfo2);
            listNames.Add(SPListName.TravelDetails2);


            foreach (string listName in listNames)
            {
                List<string> caFields = new List<string>();
                List<string> localFields = new List<string>();

                SPList list = web.Lists[listName];

                foreach (SPField field in list.Fields)
                {
                    localFields.Add(field.StaticName);
                }
                ListService.Lists listService = new ListService.Lists();
                NetworkCredential credential = new NetworkCredential("spsadmin", "ciicit#4%6", "cnaidc");
                CredentialCache cache = new CredentialCache();
                cache.Add(new Uri(ConfigurationManager.AppSettings["cnashsptest"]), "NTLM", credential);
                listService.Credentials = cache;
                listService.Url = ConfigurationManager.AppSettings["cnashsptest_listservice"];

                XmlNode listXml = listService.GetList(listName);

                XmlNodeList fieldNodes = RunXPathQuery(listXml, "//sp:Field");

                foreach (XmlNode node in fieldNodes)
                {
                    caFields.Add(node.Attributes["Name"].Value);
                }

                List<string> diff = caFields.Except(localFields).Concat(localFields.Except(caFields)).ToList();
                if (diff.Count > 0)
                {
                    Console.WriteLine(string.Format("The list {0} has the different fields, following:", listName));
                }
                foreach (string s in diff)
                {
                    Console.WriteLine(s);
                }
            }
        }

        private static XmlNodeList RunXPathQuery(XmlNode XmlNodeToQuery, string XPathQuery)
        {
            // load the complete XML node and all its child nodes into an
            // XML document
            XmlDocument Document = new XmlDocument();

            Document.LoadXml(XmlNodeToQuery.OuterXml);


            // all the possible namespaces used by SharePoint and a randomly
            // choosen prefix
            const string SharePointNamespacePrefix = "sp";
            const string SharePointNamespaceURI =
            "http://schemas.microsoft.com/sharepoint/soap/";
            const string ListItemsNamespacePrefix = "z";
            const string ListItemsNamespaceURI = "#RowsetSchema";
            const string PictureLibrariesNamespacePrefix = "y";
            const string PictureLibrariesNamespaceURI =
            "http://schemas.microsoft.com/sharepoint/soap/ois/";
            const string WebPartsNamespacePrefix = "w";
            const string WebPartsNamespaceURI =
            "http://schemas.microsoft.com/WebPart/v2";
            const string DirectoryNamespacePrefix = "d";
            const string DirectoryNamespaceURI =
            "http://schemas.microsoft.com/sharepoint/soap/directory/";
            const string DataRowSetNameSpacePrefix = "rs";
            const string DataRowSetNameSpaceURI = "urn:schemas-microsoft- com:rowset";

            // now associate with the xmlns namespaces (part of all XML
            // nodes returned from SharePoint), a namespace prefix that
            // we then can use in the queries
            XmlNamespaceManager NamespaceMngr =
            new XmlNamespaceManager(Document.NameTable);
            NamespaceMngr.AddNamespace(SharePointNamespacePrefix,
            SharePointNamespaceURI);
            NamespaceMngr.AddNamespace(ListItemsNamespacePrefix,
            ListItemsNamespaceURI);
            NamespaceMngr.AddNamespace(PictureLibrariesNamespacePrefix,
            PictureLibrariesNamespaceURI);
            NamespaceMngr.AddNamespace(WebPartsNamespacePrefix,
            WebPartsNamespaceURI);
            NamespaceMngr.AddNamespace(DirectoryNamespacePrefix,
            DirectoryNamespaceURI);
            NamespaceMngr.AddNamespace(DataRowSetNameSpacePrefix, DataRowSetNameSpaceURI);

            // run the XPath query and return the result nodes
            return Document.SelectNodes(XPathQuery, NamespaceMngr);
        }


        internal static void GetListInfo(SPWeb web)
        {
            SPList list = web.Lists[SPListName.TravelExpenseClaimDetails];

            Console.WriteLine(list.SchemaXml);
        }

        internal static void UpdateClaimState(SPWeb web)
        {
            List<SPList> spListCollection = new List<SPList>();
            spListCollection.Add(web.Lists[SPListName.TravelExpenseClaim]);
            spListCollection.Add(web.Lists[SPListName.CreditCardClaim]);
            spListCollection.Add(web.Lists[SPListName.EmployeeExpenseClaim]);
            spListCollection.Add(web.Lists[SPListName.CashAdvanceRequest]);

            foreach (SPList list in spListCollection)
            {
                Console.WriteLine(list.Title);
                DataTable dt = GetClaimState(list);
                BatchUpdatePaidStatus(list, dt);
            }

        }

        private static void BatchUpdatePaidStatus(SPList list, DataTable dt)
        {
            string listGuid = list.ID.ToString();

            StringBuilder methodBuilder = new StringBuilder();
            string batch = string.Empty;
            string batchFormat = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                                "<Batch onError=\"Return\">{0}</Batch>";

            string methodFormat = "<Method ID=\"{0}\">" +
               "<SetList Scope=\"Request\">{1}</SetList>" +
               "<SetVar Name=\"ID\">{2}</SetVar>" +
               "<SetVar Name=\"Cmd\">Save</SetVar>" +
               "<SetVar Name=\"urn:schemas-microsoft-com:office:office#IsPaid\">{3}</SetVar>" +
               "<SetVar Name=\"urn:schemas-microsoft-com:office:office#ErrorMsg\">{4}</SetVar>" +
               "</Method>";

            // Build the CAML update commands.
            foreach (DataRow dr in dt.Rows)
            {
                methodBuilder.AppendFormat(methodFormat, 0, listGuid, dr["ID"].AsString(),
                    dr["IsPaid"].AsString(),
                    dr["ErrorMsg"].AsString()
                    );
            }

            if (methodBuilder.ToString().IsNotNullOrWhitespace())
            {
                // Put the pieces together.
                batch = string.Format(batchFormat, methodBuilder.ToString());
                // Process the batch of commands.
                string batchReturn = list.ParentWeb.ProcessBatchData(batch);

                //SystemEntry.LogInfoToLogFile("Batch Info", batch, "BatchUpdatePaidStatus");
            }
        }

        private static DataTable GetClaimState(SPList list)
        {
            DataTable sourceDT = GetSAPItemsDT(list);

            List<SapParameter> mSapParametersCQ = new List<SapParameter>();

            if (sourceDT != null && sourceDT.Rows.Count > 0)
            {

                foreach (DataRow dr in sourceDT.Rows)
                {
                    string applicantSPuser = dr["ApplicantSPuser"].AsString();
                    string vendorIdStr = string.Empty;
                    decimal vendorId = 0;
                    if (applicantSPuser.IsNotNullOrWhitespace())
                    {
                        //Console.WriteLine(dr["ApplicantSPuser"].AsString());
                        //int userId = int.Parse(dr["ApplicantSPuser"].AsString().Split(new string[] { ";#" }, StringSplitOptions.None)[0]);
                        //SPUser user = list.ParentWeb.AllUsers.GetByID(userId);
                        vendorIdStr = Services.UserProfileService.GetEmployeeId(dr["ApplicantSPuser"].AsString(), list.ParentWeb);
                    }
                    if (decimal.TryParse(vendorIdStr, out vendorId))
                    {
                        #region init data
                        SapParameter mSapParameters = new SapParameter()
                        {
                            RefDocNo = dr["Title"].AsString(),
                            Vendor = string.Format("{0:0000000000}", vendorId)
                        };
                        mSapParametersCQ.Add(mSapParameters);

                        #endregion
                    }
                }

                ISapExchange sapClaim = SapExchangeFactory.GetClaimQuer();
                List<object[]> claimResult = sapClaim.ExportDataToCa(mSapParametersCQ);

                #region update result
                for (int i = 0; i < claimResult.Count; i++)
                {
                    SapParameter sapPara = (SapParameter)claimResult[i][0];
                    foreach (DataRow dr in sourceDT.Rows)
                    {
                        if (dr["Title"].AsString().Equals(sapPara.RefDocNo.ToString(), StringComparison.CurrentCultureIgnoreCase))
                        {
                            if ((bool)claimResult[i][2])
                            {
                                SapResult sr = (SapResult)claimResult[i][1];
                                Claim claimStatus = ((SapResult)claimResult[i][1]).ClaimStatus;

                                Console.WriteLine("WFNo:{0},Result:{1}", dr["Title"].AsString(), claimStatus.ZFLAG.ToString());
                                SystemEntry.LogInfoToLogFile(string.Format("WFNo:{0},Result:{1}", dr["Title"].AsString(), claimStatus.ZFLAG.ToString()),
                                                             string.Empty,
                                                             "GetClaimState");

                                if (claimStatus.ZFLAG.Equals("X", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    dr["IsPaid"] = "1";
                                }
                            }
                            else
                            {
                                Console.WriteLine("WFNo:{0},Result:{1}", dr["Title"].AsString(), bool.FalseString);
                                if (claimResult[i][1] is string)
                                {
                                    Console.WriteLine("WFNo:{0},Result:{1}", dr["Title"].AsString(), claimResult[i][1].ToString());
                                    dr["ErrorMsg"] += claimResult[i][1].ToString();
                                }
                                else
                                {
                                    SapResult sr = (SapResult)claimResult[i][1];
                                    Console.WriteLine("WFNo:{0},Result:{1}", dr["Title"].AsString(), sr.ClaimStatus);
                                }
                            }
                            break;
                        }
                    }

                }
                #endregion
            }

            return sourceDT;
        }

        private static DataTable GetSAPItemsDT(SPList list)
        {
            DataTable dt = null;
            SPQuery query;

            switch (list.Title)
            {
                case SPListName.TravelExpenseClaim:
                    DataTable sourceDT = list.Items.GetDataTable();
                    var matches = from dr in sourceDT.AsEnumerable()
                                  where HasPostToSAP(dr, list)
                                  select dr;
                    dt = matches.AsDataView().ToTable();
                    break;
                case SPListName.EmployeeExpenseClaim:
                    query = new SPQuery();
                    query.Query = @" <Where>
                                      <IsNotNull>
                                         <FieldRef Name='SAPNumber' />
                                      </IsNotNull>
                                   </Where>";
                    dt = list.GetItems(query).GetDataTable();
                    break;
                case SPListName.CreditCardClaim:
                    query = new SPQuery();
                    query.Query = @" <Where>
                                          <Or>
                                             <IsNotNull>
                                                <FieldRef Name='SAPNo' />
                                             </IsNotNull>
                                             <IsNotNull>
                                                <FieldRef Name='SAPUSDNo' />
                                             </IsNotNull>
                                          </Or>
                                       </Where>";
                    dt = list.GetItems(query).GetDataTable();
                    break;
                default:
                    query = new SPQuery();
                    query.Query = @" <Where>
                                      <IsNotNull>
                                         <FieldRef Name='SAPNo' />
                                      </IsNotNull>
                                   </Where>";
                    dt = list.GetItems(query).GetDataTable();
                    break;
            }

            return dt;
        }

        private static bool HasPostToSAP(DataRow dr, SPList list)
        {
            bool hasPostToSAP = false;
            SPQuery query = new SPQuery();
            query.Query = string.Format(@"<Where>
                                              <Eq>
                                                 <FieldRef Name='TCWorkflowNumber' />
                                                 <Value Type='Text'>{0}</Value>
                                              </Eq>
                                           </Where>", dr["Title"].AsString());
            SPListItemCollection items = list.ParentWeb.Lists[SPListName.TravelExpenseClaimForSAP].GetItems(query);
            if (items.Count > 0)
            {
                hasPostToSAP = items[0]["SAPNo"].AsString().IsNotNullOrWhitespace();
            }

            return hasPostToSAP;
        }

        internal static void UpdateTravelExpenseClaimWFNo(SPWeb web)
        {
            Hashtable hash = new Hashtable();
            hash.Add("TEProject Management Office0001", "TE000006");

            foreach (DictionaryEntry entry in hash)
            {
                string queryFormat = @"<Where>
                                <Eq>
                                    <FieldRef Name='Title' />
                                    <Value Type='Text'>{0}</Value>
                                </Eq>
                             </Where>";

                SPListItemCollection travelExpenseItems = GetListItemsByQuery(web.Lists[SPListName.TravelExpenseClaim], queryFormat, entry.Key.AsString());
                foreach (SPListItem travelExpenseItem in travelExpenseItems)
                {
                    travelExpenseItem["Title"] = entry.Value.AsString();
                    travelExpenseItem.SystemUpdate();
                }

                SPListItemCollection travelExpenseDetailsItems = GetListItemsByQuery(web.Lists[SPListName.TravelExpenseClaimDetails], queryFormat, entry.Key.AsString());
                foreach (SPListItem travelExpenseDetailsItem in travelExpenseDetailsItems)
                {
                    travelExpenseDetailsItem["Title"] = entry.Value.AsString();
                    travelExpenseDetailsItem.SystemUpdate();
                }
            }
        }

        private static SPListItemCollection GetListItemsByQuery(SPList list, string queryFormat, params string[] values)
        {
            SPQuery query = new SPQuery();
            query.Query = string.Format(queryFormat, values);

            return list.GetItems(query);
        }

        internal static void UpdateSpecificTRClaimLink(SPWeb web)
        {
            SPList list = web.Lists[SPListName.TravelRequestWorkflow2];

            foreach (SPListItem item in list.Items)
            {
                if (item.ID == 2140)
                {
                    SPFieldUrlValue link = item.Fields["Claim"].GetFieldValue(item["Claim"].AsString()) as SPFieldUrlValue;
                    if (link != null && link.Description == "Claim")
                    {
                        Console.WriteLine(item["Title"].AsString());
                        SPFieldUrlValue newLink = new SPFieldUrlValue();
                        newLink.Description = "Closed";
                        //newLink.Url = string.Format("https://portal.c-and-a.cn/WorkFlowCenter/Lists/TravelExpenseClaim/NewForm.aspx?TRNumber={0}", HttpUtility.UrlEncode(item["Title"].AsString()));
                        item["Claim"] = newLink;
                        item.SystemUpdate();
                        //break;
                    }
                }
            }

        }

        internal static void UpdateTRClaimLink(SPWeb web)
        {
            List<string> updateItemQueue = new List<string>();

            SPList TRlist = web.Lists[SPListName.TravelRequestWorkflow2];
            SPList TEList = web.Lists[SPListName.TravelExpenseClaim];

            foreach (SPListItem item in TRlist.Items)
            {
                SPListItemCollection items = GetListItemsByQuery(TEList, @"<Where>
                                                                            <And>
                                                                                <Eq>
                                                                                <FieldRef Name='TRWorkflowNumber' />
                                                                                <Value Type='Text'>{0}</Value>
                                                                                </Eq>
                                                                                <Neq>
                                                                                <FieldRef Name='Status' />
                                                                                <Value Type='Text'>{1}</Value>
                                                                                </Neq>
                                                                            </And>
                                                                        </Where>",
                                                                        item[SPBuiltInFieldId.Title].AsString(),
                                                                        "Pending");

                if (items.Count > 0)
                {
                    SPFieldUrlValue link = item.Fields["Claim"].GetFieldValue(item["Claim"].AsString()) as SPFieldUrlValue;
                    if (link != null && link.Description == "Claim")
                    {
                        Console.WriteLine(item["Title"].AsString());
                        updateItemQueue.Add(item[SPBuiltInFieldId.Title].AsString());
                        SPFieldUrlValue newLink = new SPFieldUrlValue();
                        ////newLink.Description = "Claim";
                        ////newLink.Url = string.Format("https://portal.c-and-a.cn/WorkFlowCenter/Lists/TravelExpenseClaim/NewForm.aspx?TRNumber={0}", HttpUtility.UrlEncode(item["Title"].AsString()));

                        newLink.Description = "Closed";
                        newLink.Url = "https://portal.c-and-a.cn/WorkFlowCenter/Lists/TravelExpenseClaim/MyApply.aspx";
                        item["Claim"] = newLink;
                        item.SystemUpdate();
                        //break;
                    }
                }
            }
            //SystemEntry.LogInfoToLogFile("The UpdateItem Queue", string.Join("\r\n", updateItemQueue.ToArray()), "UpdateTRClaimLink");
            //Console.WriteLine(string.Join(",", updateItemQueue.ToArray()));
        }

        internal static void ReassignWorkflowTask(SPWeb web)
        {
            //224 20213
            //239 20782

            int itemId = 2066;

            int taskId = 43866;
            int reassignUserId = 467;
            SPList list = web.Lists[SPListName.TravelRequestWorkflow2];

            SPListItem item = list.GetItemById(itemId);

            foreach (SPWorkflowTask task in item.Tasks)
            {
                if (task.ID.Equals(taskId))
                {
                    web.AllowUnsafeUpdates = true;
                    Hashtable ht = new Hashtable();
                    //ht.Add("AssignedTo",web.EnsureUser(@"micro\ituser"));
                    ht.Add(SPBuiltInFieldId.WorkflowOutcome, string.Empty);
                    ht.Add("Status", SPUtility.GetLocalizedString("$Resources:Tasks_NotStarted;", "core", web.Language));
                    ht.Add("__ReAssignUser", web.Users.GetByID(reassignUserId));
                    ht.Add("__Action", "ReAssign");

                    SPWorkflowTask.AlterTask(task as SPListItem, ht, true);
                    Console.WriteLine("Successful!");
                }
            }



        }

        internal static void RetryingWF(SPWeb web)
        {
            int itemId = 836;
            int taskId = 31610;
            string accountName = @"CNAIDC\Marco.Hamers";
            SPList list = web.Lists[SPListName.PurchaseRequest];

            SPListItem item = list.GetItemById(itemId);

            foreach (SPWorkflowTask task in item.Tasks)
            {
                if (task.ID.Equals(taskId))
                {
                    SPWorkflow workflow = item.Workflows[task.WorkflowId];
                    //web.Site.WorkflowManager.ModifyWorkflow(workflow, workflow.Modifications[0], null);

                    Console.WriteLine(string.Format("Workflow Internal Satae:{0},Is Locked:{1},Is Completed:{2}", workflow.InternalState, workflow.IsLocked, workflow.IsCompleted));
                    SystemEntry.LogEventInfo(workflow.Xml);
                    //web.AllowUnsafeUpdates = true;
                    //Hashtable ht = new Hashtable();
                    //ht.Add("AssignedTo", web.EnsureUser(accountName));
                    //ht.Add(SPBuiltInFieldId.WorkflowOutcome, "Approve");
                    ////ht.Add("Status", SPUtility.GetLocalizedString("$Resources:Tasks_Completed;", "core", web.Language));
                    ////ht.Add("TaskStatus", "#");

                    //SPWorkflowTask.AlterTask(task as SPListItem, ht, true);

                    Console.WriteLine("Successful!");
                }
            }


        }

        internal static void GetDataTable(SPWeb web)
        {
            SPList list = web.Lists[SPListName.Tasks];
            DataTable dt = list.Items.GetDataTable();

            foreach (SPListItem item in list.Items)
            {
                if (item[SPBuiltInFieldId.TaskStatus].AsString() == "Completed")
                {
                    continue;
                }
                else if (item[SPBuiltInFieldId.TaskStatus].AsString() == "Not Started")
                {
                    continue;
                }
            }

        }
    }
}
