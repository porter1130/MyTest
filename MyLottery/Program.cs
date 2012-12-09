using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using System.Data.SqlClient;
using System.Threading;
using System.Data;

namespace MyLottery
{
    class Program
    {
        static string latestAwardIssue = default(string);
        static Queue<int> redBallQueue = new Queue<int>();
        static Queue<string> redBallStrQueue = new Queue<string>();
        static int blueBall = default(int);
        static string connectionString = @"Data Source=(local);Initial Catalog=MyLottery;Integrated Security=true";

        static void Main(string[] args)
        {
            DataTable lotteryHistoryDT = GetLotteryHistoryData("http://www.17500.cn/ssq/details.php", 2003);
            PersistLotteryHistoryData(lotteryHistoryDT);
            //string pageSource = GetPageSource("http://caipiao.taobao.com/lottery/order/lottery_ssq.htm");

            //ParseLatestAwardInfo(pageSource);

            //PersistLatestAwardInfo();
        }

        private static void PersistLotteryHistoryData(DataTable lotteryHistoryDT)
        {
            try
            {
                SqlBulkCopy sqlBulk = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction);
                sqlBulk.SqlRowsCopied += new SqlRowsCopiedEventHandler(sqlBulk_SqlRowsCopied);
                sqlBulk.NotifyAfter = lotteryHistoryDT.Rows.Count;

                sqlBulk.DestinationTableName = "LotteryHistory";

                sqlBulk.ColumnMappings.Add(0, "Issue");
                sqlBulk.ColumnMappings.Add(1, "RedOne");
                sqlBulk.ColumnMappings.Add(2, "RedTwo");
                sqlBulk.ColumnMappings.Add(3, "RedThree");
                sqlBulk.ColumnMappings.Add(4, "RedFour");
                sqlBulk.ColumnMappings.Add(5, "RedFive");
                sqlBulk.ColumnMappings.Add(6, "RedSix");
                sqlBulk.ColumnMappings.Add(7, "Blue");

                sqlBulk.WriteToServer(lotteryHistoryDT);
                sqlBulk.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                lotteryHistoryDT.Dispose();
            }
        }

        static void sqlBulk_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs args)
        {
            Console.WriteLine("Import {0} records", args.RowsCopied.ToString());
        }

        private static DataTable GetLotteryHistoryData(string pageUrl, int start)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("issue");
            dt.Columns.Add("redone");
            dt.Columns.Add("redtwo");
            dt.Columns.Add("redthree");
            dt.Columns.Add("redfour");
            dt.Columns.Add("redfive");
            dt.Columns.Add("redsix");
            dt.Columns.Add("blue");

            for (int i = start; i <= DateTime.Now.Date.Year; i++)
            {
                for (int j = 1; j <= 999; j++)
                {
                    string param = string.Format("{0}{1:000}", i, j);
                    Console.WriteLine(param);
                    string url = string.Format("{0}?issue={1}", pageUrl, param);

                    if (!ParseLotteryHistory(url))
                    {
                        break;
                    }

                    dt.Rows.Add(param,
                                redBallQueue.Dequeue(),
                                redBallQueue.Dequeue(),
                                redBallQueue.Dequeue(),
                                redBallQueue.Dequeue(),
                                redBallQueue.Dequeue(),
                                redBallQueue.Dequeue(),
                                blueBall);
                }
                

            }

            return dt;

        }

        private static bool ParseLotteryHistory(string url)
        {
            bool isValid = false;
            string pageSource = GetPageSource(url);

            var htmlDoc = new HtmlDocument
            {
                OptionFixNestedTags = true,
                OptionAutoCloseOnEnd = true
            };

            htmlDoc.LoadHtml(pageSource);

            if (htmlDoc.DocumentNode != null)
            {
                var ballNodes = htmlDoc.DocumentNode.SelectNodes("/html[1]/body[1]/center[1]/center[1]/table[1]/tr[1]/td[1]/table[2]/tbody[1]/tr[2]/td");

                if (ballNodes != null)
                {
                    isValid = true;
                    foreach (HtmlNode ballNode in ballNodes)
                    {
                        if (ballNode.Attributes["align"].Value.Equals("middle"))
                        {
                            redBallQueue.Enqueue(int.Parse(ballNode.InnerText.Trim()));
                        }
                        else
                        {
                            blueBall = int.Parse(ballNode.InnerText.Trim());
                        }
                    }
                }

            }

            return isValid;
        }

        private static void PersistLatestAwardInfo()
        {

            if (!ExistsIssue(latestAwardIssue))
            {

                string queryString = @"insert into LotteryHistory
                                   values(@issue,@redOne,@redTwo,@redThree,@redFour,@redFive,@redSix,@blue)";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, conn);
                    command.Parameters.AddWithValue("@issue", latestAwardIssue);
                    command.Parameters.AddWithValue("@redOne", redBallQueue.Dequeue());
                    command.Parameters.AddWithValue("@redTwo", redBallQueue.Dequeue());
                    command.Parameters.AddWithValue("@redThree", redBallQueue.Dequeue());
                    command.Parameters.AddWithValue("@redFour", redBallQueue.Dequeue());
                    command.Parameters.AddWithValue("@redFive", redBallQueue.Dequeue());
                    command.Parameters.AddWithValue("@redSix", redBallQueue.Dequeue());
                    command.Parameters.AddWithValue("@blue", blueBall);

                    try
                    {
                        conn.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private static bool ExistsIssue(string latestAwardIssue)
        {
            bool isExist = false;
            string queryString = @"select * from LotteryHistory
                                  where issue=@latestAwardIssue";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, conn);
                command.Parameters.AddWithValue("@latestAwardIssue", latestAwardIssue);

                try
                {
                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    isExist = reader.HasRows;
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return isExist;
        }

        private static void ParseLatestAwardInfo(string pageSource)
        {
            while (string.IsNullOrEmpty(latestAwardIssue))
            {
                var htmlDoc = new HtmlDocument
                {
                    OptionFixNestedTags = true,
                    OptionAutoCloseOnEnd = true
                };

                htmlDoc.LoadHtml(pageSource);

                if (htmlDoc.DocumentNode != null)
                {
                    HtmlNode latestAwardNode = htmlDoc.GetElementbyId("J-issue-now");

                    if (latestAwardNode != null)
                    {
                        HtmlNode latestAwardIssueNode = latestAwardNode.SelectSingleNode(latestAwardNode.XPath + "/tr[1]/td[1]/span[1]");
                        latestAwardIssue = latestAwardIssueNode.InnerText.Trim();
                        var latestAwardBallNodes = latestAwardNode.SelectNodes(latestAwardNode.XPath + "//td//span")
                                                                      .Where(x => x.ParentNode.HasAttributes);


                        foreach (HtmlNode ballNode in latestAwardBallNodes)
                        {
                            if (!ballNode.HasAttributes)
                            {
                                redBallQueue.Enqueue(int.Parse(ballNode.InnerText.Trim()));
                            }
                            else
                            {
                                blueBall = int.Parse(ballNode.InnerText.Trim());
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Fetch latest award issue info failed, retrying after 5 seconds.");
                        Thread.Sleep(5000);
                    }
                }
            }
        }

        private static string GetPageSource(string pageUrl)
        {
            string charset = "gb2312";
            string pageSource = string.Empty;
            try
            {
                string strHtml = string.Empty;
                HttpWebRequest request = WebRequest.Create(pageUrl) as HttpWebRequest;
                request.Method = "Get";
                request.KeepAlive = true;

                HttpWebResponse response = null;
                try
                {
                    response = request.GetResponse() as HttpWebResponse;
                }
                catch (WebException ex)
                {
                    response = ex.Response as HttpWebResponse;
                }

                Stream s = response.GetResponseStream();
                StreamReader reader = new StreamReader(s, Encoding.GetEncoding(charset));

                pageSource = reader.ReadToEnd();
            }
            catch (Exception exception)
            {
                pageSource = exception.Message;
            }

            return pageSource;
        }


    }
}
