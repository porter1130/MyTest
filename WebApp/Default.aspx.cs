using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace WebApp
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //string path = "About.aspx?id=" + Server.UrlEncode("TRTEST&IT");
            //Server.Transfer(path);
            if (!this.IsPostBack)
            {
                RepeaterTableDataBind();
            }
        }

        private void RepeaterTableDataBind()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("A");
            dt.Columns.Add("B");
            dt.Columns.Add("C");

            dt.Rows.Add(new string[] { "A1", "B1", "C1" });
            dt.Rows.Add(new string[] { "A2", "B2", "C2" });
            dt.Rows.Add(new string[] { "A3", "B3", "C3" });

            rptTable.DataSource = dt;
            rptTable.DataBind();
        }

        protected void rptTable_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName.Equals("delete", StringComparison.CurrentCultureIgnoreCase))
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("A");
                dt.Columns.Add("B");
                dt.Columns.Add("C");

                dt.Rows.Add(new string[] { "A1", "Z1", "C1" });
                dt.Rows.Add(new string[] { "A2", "H2", "C2" });
                dt.Rows.Add(new string[] { "A3", "S3", "C3" });
                dt.Rows.Add(new string[] { "A4", "", "C4" });
                rptTable.DataSource = GetDataSource(dt, "B");
                rptTable.DataBind();
            }
        }

        private object GetDataSource(DataTable sourceTable, string sort)
        {
            DataTable dt = null;
            if (sourceTable != null)
            {
                dt = sourceTable.Clone();
                DataRow[] rows = sourceTable.Select(null, sort);

                if (rows != null)
                {
                    foreach (DataRow row in rows)
                    {
                        dt.ImportRow(row);
                    }
                }
            }

            return dt;
        }

    }
}
