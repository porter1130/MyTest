using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.SharePoint;
using System.Web.Script.Serialization;
using System.Runtime.Serialization;
using System.Reflection;
using System.Collections;
using System.Data.OleDb;
using System.Data;
using System.Text.RegularExpressions;

#region Console
using ConsoleApp.Module;
using ConsoleApp.Constants;
using ConsoleApp.Extensions;
using ConsoleApp.Object;
#endregion

using System.Globalization;

namespace ConsoleApp
{
    class Program
    {
        public static string[] workSheetNames = new string[] { };   //List of Worksheet names 
        private static string fileName;
        private static string connectionString;

        private static string siteUrl = "http://porter:1130/WorkflowCenter";


        static void Main(string[] args)
        {
            //siteUrl = "http://cnashsptest.cnaidc.cn:91/WorkflowCenter";
            //siteUrl = "http://172.17.1.45:83/WorkflowCenter";
            //siteUrl = "https://cnshsps.cnaidc.cn/WorkflowCenter";
            //siteUrl = "http://caserver:9001/WorkflowCenter";

            //TypeEntry.ConvertToBool("true");         

            Console.WriteLine("Start");
            #region SharePoint Entry
            using (SPSite site = new SPSite(siteUrl))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    try
                    {
                        #region Common Operations

                        CommonObjectEntry.TestContain();
                        //SharePointEntry.GetCEOName(web);
                        //CommonObjectEntry.GetDateInfo(web);
                        //CommonObjectEntry.TestStringFormat(web);
                        //CommonObjectEntry.TestContain(web);
                        //CommonObjectEntry.ListContainsExtension();

                        #endregion

                        #region SPList Operations
                        //SharePointEntry.UpdateClaimState(web);
                        //SharePointEntry.UpdateTravelExpenseClaimWFNo(web);
                        //SharePointEntry.FindListByTemplateFeatureName(web);
                        //SharePointEntry.SaveListTemplateByFeatureName(web);
                        //SharePointEntry.GetListInfo(web);
                        //SharePointEntry.UpdateTRClaimLink(web);
                        //SharePointEntry.UpdateSpecificTRClaimLink(web);
                        #endregion

                        #region SPField Operations
                        //SharePointEntry.GetTaskField(web);
                        //SharePointEntry.GetSPFieldUserValue(web);
                        //SharePointEntry.ExamDiffFieldsByListService(web);

                        #endregion

                        #region SPWorkflow Operations

                        //SharePointEntry.GetDataTable(web);
                        //SharePointEntry.StartWorkflowInstance(web);
                        //SharePointEntry.GetWorkflowInfo(web);
                        //SharePointEntry.FindLockedWorkflow(web);
                        //SharePointEntry.UnLockWorkflowTasks(web);
                        //SharePointEntry.UpdateReadOnlyField(web);
                        //SharePointEntry.GetTaskListItemInfo(web);
                        //SharePointEntry.InsertListItemToTask(web);
                        //SharePointEntry.UpdateTaskListItem(web);
                        //SharePointEntry.TerminateWorkflowTask(web);
                        //SharePointEntry.ReassignWorkflowTask(web);
                        //SharePointEntry.RetryingWF(web);

                        #endregion

                        #region DataTable Operations
                        //DataTableEntry.LoadDataRow();
                        //DataTableEntry.LinqGroupBy(web);

                        //string a="www";
                        //SPList list=web.Lists[SPListName.TravelExpenseClaimDetails];
                        //DataTableEntry.GetDataSource(list.Items.GetDataTable(),"Title='"+a+"'");

                        //DataTableEntry.ConvertToSAPDataSource(web);
                        #endregion

                        #region System Operations
                        //SystemEntry.LogEventInfo("test3");
                        #endregion

                        #region List Operations
                        //CommonObjectEntry.ListContainsExtension();

                        #endregion

                        #region XML Operations
                        //XMLEntry.SerializeWorkflowVariable(web);

                        #endregion

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.Source);
                        Console.WriteLine(ex.StackTrace);
                        Console.WriteLine(ex.GetType());
                        SystemEntry.LogInfoToLogFile("ConsoleApp error occurs",
                                                     string.Format("[Exception Message]:{0}\n [Exception Source]:{1}\n [Exception StackTrace]:{2}\n [Exception Type]:{3}",
                                                                    ex.Message,
                                                                    ex.Source,
                                                                    ex.StackTrace,
                                                                    ex.GetType()),
                                                      "Main");

                    }
                }
            }
            #endregion

            Console.WriteLine("End");
            Console.ReadLine();
        }

        private static int ToNumber(string primaryKeyValue)
        {
            char[] tags = primaryKeyValue.ToCharArray();
            int index = 0;
            if (tags.Length == 1)
            {
                index = (int)tags[0] % (int)'A';
            }
            else
            {
                int position = 0;
                for (int i = 0; i < tags.Length; i++)
                {
                    position += (ToNumber(tags[i].ToString()) + 1) * (int)Math.Pow(26, tags.Length - 1 - i);
                }
                index = position - 1;
            }
            return index;
        }

        private static string ToName(int index)
        {
            if (index < 0) { throw new Exception("invalid parameter"); }

            List<string> chars = new List<string>();
            do
            {
                if (chars.Count > 0) index--;
                chars.Insert(0, ((char)(index % 26 + (int)'A')).ToString());
                index = (int)((index - index % 26) / 26);
            } while (index > 0);

            return String.Join(string.Empty, chars.ToArray());
        }

        private static void OpenExcelFile(bool isOpenXMLFormat)
        {
            //open the excel file using OLEDB
            OleDbConnection con;

            if (isOpenXMLFormat)
                //read a 2007 file
                connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                    fileName + ";Extended Properties=\"Excel 8.0;HDR=YES;\"";
            else
                //read a 97-2003 file
                connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                    fileName + ";Extended Properties=Excel 8.0;";

            con = new OleDbConnection(connectionString);
            con.Open();

            //get all the available sheets
            System.Data.DataTable dataSet = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

            //get the number of sheets in the file
            workSheetNames = new String[dataSet.Rows.Count];
            int i = 0;
            foreach (DataRow row in dataSet.Rows)
            {
                //insert the sheet's name in the current element of the array
                //and remove the $ sign at the end
                workSheetNames[i] = row["TABLE_NAME"].ToString().Trim(new[] { '$' });
                i++;
            }

            if (con != null)
            {
                con.Close();
                con.Dispose();
            }

            if (dataSet != null)
                dataSet.Dispose();
        }

    }
    public class TravelPolicyItem
    {
        public string Title { get; set; }
        public string City { get; set; }
        public string Currency { get; set; }
        public string Location { get; set; }
        public string TotalMealAllowance { get; set; }
        public List<string> ID { get; set; }
    }
}

