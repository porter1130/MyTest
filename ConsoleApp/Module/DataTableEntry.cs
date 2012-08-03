using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using ConsoleApp.Constants;
using System.Data;
using System.Collections;
using ConsoleApp.Extensions;

namespace ConsoleApp.Module
{
    class DataTableEntry
    {
        private static List<object> dataList = new List<object>();
        internal static DataTable GetDataSource(DataTable sourceTable, string exp)
        {
            DataTable targetDataTable = sourceTable.Clone();
            DataRow[] rows = sourceTable.Select(exp);

            foreach (DataRow row in rows)
            {
                targetDataTable.ImportRow(row);
            }

            return targetDataTable;
        }

        internal static void LinqGroupBy(SPWeb web)
        {
            SPList list = web.Lists[SPListName.TravelExpenseClaimDetails];

            DataTable dt = list.Items.GetDataTable();

            var query = from e in dt.AsEnumerable()
                        group e by new { ExpenseType = e["ExpenseType"].ToString(), CostCenter = e["CostCenter"].ToString() } into grp
                        select new
                        {
                            ExpenseType = grp.Key.ExpenseType,
                            CostCenter = grp.Key.CostCenter,
                            SubTotalCost = grp.Sum(cost => Decimal.Parse(string.IsNullOrEmpty(cost["ApprovedRmbAmt"].ToString()) ? "0" : cost["ApprovedRmbAmt"].ToString()))
                        };

            DataTable source = query.AsDataTable();

            source.WriteXml(Console.Out);
        }

        internal static void LoadDataRow()
        {
            DataTable dt = new DataTable();

            DataColumn dc1 = new DataColumn("col1");
            DataColumn dc2 = new DataColumn("col2");
            DataColumn dc3 = new DataColumn("col3");



            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);
            dt.Columns.Add(dc3);

            dt.Rows.Add("1", "2", "3");
            dt.Rows.Add("1", "2", "3");
            dt.Rows.Add("1", "2", "3");

            Copy(dt);

            dt.Clear();

            //dt.PrimaryKey = new DataColumn[] { dc1 };
            //// Create an array for the values.
            //object[] newRow = new object[3];

            //// Set the values of the array.
            //newRow[0] = "Hello";
            //newRow[1] = "World";
            //newRow[2] = "two";

            //// Create an array for the values.
            //object[] newRow2 = new object[3];

            //// Set the values of the array.
            //newRow2[0] = "Hello";
            //newRow2[1] = "World";
            //newRow2[2] = "two2";
            //DataRow row;


            //dt.BeginLoadData();
            //// Add the new row to the rows collection.
            //row = dt.LoadDataRow(newRow, false);
            //row = dt.LoadDataRow(newRow2, false);

            ////dt.EndLoadData();
            //foreach (DataRow dr in dt.Rows)
            //{
            //    Console.WriteLine(String.Format("Row: {0}, {1}, {2}", dr["col1"], dr["col2"], dr["col3"]));
            //}

        }

        private static void Copy(DataTable dt)
        {

            dataList.Add(dt);
        }

        internal static void ConvertToSAPDataSource(SPWeb web)
        {
            string type = "RMB";

            DataTable dataTable = web.Lists[SPListName.CreditCardBill].Items.GetDataTable();

            var query = from e in dataTable.AsEnumerable()
                        where e["Currency"].AsString() == type
                        select new
                        {
                            RefKey = e["ID"].AsString(),
                            Amount = decimal.Parse(e["PayAmt"].AsString()) - decimal.Parse(e["DepositAmt"].AsString()),
                            ItemText = e["MerchantName"].AsString()
                        };

            DataTable dt = query.AsDataTable();

            switch (type)
            {
                case "RMB":

                    break;
                case "USD":

                    break;
                default:
                    break;
            }

        }
    }
}
