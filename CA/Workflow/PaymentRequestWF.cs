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

            string eventData = @"<?xml version='1.0' encoding='utf-8'?><Data><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>StoreAdminSubmitTitle</string></key><value><string>Please complete store sampling</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>BuyerApproveTitle</string></key><value><string>Sophie Cheng's Store Sampling needs confirm</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>StoreManagerApproveTitle</string></key><value><string>Sophie Cheng's Store Sampling needs approval</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>AreaManagerApproveTitle</string></key><value><string>Sophie Cheng's Store Sampling needs approval</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>BSSHeadGroupApproveTitle</string></key><value><string>Sophie Cheng's Store Sampling needs approval</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>BSSTeamTitle</string></key><value><string>Sophie Cheng's Store Sampling needs approval</string></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>FinanceGroupConfirmTitle</string></key><value><string>Sophie Cheng's Store Sampling needs confirm</string></value></item><item type='QuickFlow.NameCollection, QuickFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ec1e0fe6e1745628'><key><string>StoreManager</string></key><value><ArrayOfString xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><string>CNAIDC\Sophie.Cheng</string></ArrayOfString></value></item><item type='QuickFlow.NameCollection, QuickFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ec1e0fe6e1745628'><key><string>AreaManagerApproveUser</string></key><value><ArrayOfString xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><string>CNAIDC\rose.tan</string></ArrayOfString></value></item><item type='QuickFlow.NameCollection, QuickFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ec1e0fe6e1745628'><key><string>Buyer</string></key><value><ArrayOfString xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><string>CNAIDC\vincent.zhang</string></ArrayOfString></value></item><item type='QuickFlow.NameCollection, QuickFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ec1e0fe6e1745628'><key><string>BSSHeadGroup</string></key><value><ArrayOfString xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><string>CNAIDC\michiel.b</string></ArrayOfString></value></item><item type='QuickFlow.NameCollection, QuickFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ec1e0fe6e1745628'><key><string>BSSTeamAccount</string></key><value><ArrayOfString xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><string>CNAIDC\angela.xu</string><string>CNAIDC\yan.yang</string><string>CNAIDC\judith.zhang</string></ArrayOfString></value></item><item type='QuickFlow.NameCollection, QuickFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ec1e0fe6e1745628'><key><string>FinanceGroup</string></key><value><ArrayOfString xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><string>CNAIDC\rachel.chen</string></ArrayOfString></value></item><item type='System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'><key><string>IsSubmit</string></key><value><string>Yes</string></value></item></Data>";

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

            string eventData = @"<?xml version='1.0' encoding='utf-8'?><Data><item type='QuickFlow.NameCollection, QuickFlow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ec1e0fe6e1745628'><key><string>FinanceGroup</string></key><value><ArrayOfString xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><string>CNAIDC\rachel.chen</string></ArrayOfString></value></item></Data>";

            #region alter task
            Hashtable hash = new Hashtable();
            hash.Add("__WorkflowVaribales", eventData);
            hash.Add("__TaskOutcome", "Approve");
            //hash.Add("__TaskOutcome", "Approve");
            //hash.Add("__TaskOutcome", "Reject");
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
