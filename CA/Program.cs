using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Microsoft.SharePoint;

namespace CA
{
    class Program
    {
        private static string siteUrl = MOSSSiteUrl.Production_Environment;

        static void Main(string[] args)
        {
            Console.WriteLine("Begin");

            using (SPSite site = new SPSite(siteUrl))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    try
                    {
                        IWorkflowFactory workflowFactory = new TEWorkflowFactory();
                        MOSSContext context = MOSSContext.GetInstance(web);
                        //BaseWF baseWF = null;
                        //foreach (SPList list in web.Lists)
                        //{
                        //    if (list.Title.Equals(SPListName.PurchaseRequest))
                        //    {
                        //        context.CurrList = list;
                        //        context.WfName = context.GetLastedWorkflowName();
                        //        if (!string.IsNullOrEmpty(context.WfName))
                        //        {
                        //            baseWF = workflowFactory.CreateWF(context);
                        //            baseWF.AutoUnLock();
                        //        }
                        //    }
                        //}
                        //if (baseWF != null)
                        //{
                        //    baseWF.SendAlertMail();
                        //}


                        context.CurrList = web.Lists[SPListName.TravelExpenseClaim];
                        context.WfName = context.GetLastedWorkflowName();
                        //context.WfName = WorkflowName.NonTradeSupplierSetupMaintenanceWF2;
                        context.ApplicantSPUser = web.AllUsers.GetByID(461);
                        BaseWF baseWF = workflowFactory.CreateWF(context);

                        //baseWF.StartWorkflowInstance(434);
                        ////baseWF.CancelWorkflowInstance(1131);
                        baseWF.RetryingWF(58170);
                        ////baseWF.ReassignWorkflowTask(68, 42452, 751);
                        //baseWF.UnLockSpecificWF(266);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format("Exception:{0}", ex.Message));
                        Console.WriteLine(string.Format("StackTrace:{0}", ex.StackTrace));
                    }
                }
            }

            Console.WriteLine("End");
            Console.Read();

        }
    }
}
