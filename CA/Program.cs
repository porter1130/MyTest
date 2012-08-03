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
                        IWorkflowFactory workflowFactory = new PRWorkflowFactory();
                        MOSSContext context = MOSSContext.GetInstance(web);
                        //BaseWF baseWF = null;
                        //foreach (SPList list in web.Lists)
                        //{
                        //    context.CurrList = list;
                        //    context.WfName = context.GetLastedWorkflowName();
                        //    if (!string.IsNullOrEmpty(context.WfName))
                        //    {
                        //        baseWF = workflowFactory.CreateWF(context);
                        //        baseWF.AutoUnLock();
                        //    }
                        //}
                        //if (baseWF != null)
                        //{
                        //    baseWF.SendAlertMail();
                        //}
                        context.CurrList = web.Lists[SPListName.PADChangeRequest];
                        context.WfName = context.GetLastedWorkflowName();
                        BaseWF baseWF = workflowFactory.CreateWF(context);

                        //baseWF.StartWorkflowInstance(1131);
                        //baseWF.CancelWorkflowInstance(1131);
                        //baseWF.RetryingWF(48844);
                        baseWF.ReassignWorkflowTask(3204, 48414, 409);
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
