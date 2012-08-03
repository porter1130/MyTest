using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleApp.Extensions;
using Microsoft.SharePoint;

namespace ConsoleApp.Module
{
    class CommonObjectEntry
    {
        private static List<string> stringList = new List<string>();
        internal static void ListContainsExtension()
        {
            List<string> list = new List<string>() { "Abc", "Mark" };
            string str = "Abckdf";
            string sdf = "abc";
            //if (list.Contains("abc", new MyComparer.MyCaseInsensitiveComparer()))
            //{
            //    Console.WriteLine("true");
            //};
            if (str.ToLowerInvariant().Contains(sdf.ToLowerInvariant()))
            {
                Console.WriteLine(true);
            }
        }

        internal static void GetDateInfo(Microsoft.SharePoint.SPWeb web)
        {
            DateTime curDate = DateTime.Now.AddYears(-1);
            string dateStr = "2011-08";
            DateTime date = DateTime.Parse(dateStr);
            Console.WriteLine(date.AddMonths(-1).ToString("yyyy-MM"));
            Console.WriteLine(DateTime.Now.ToString("yyyyMMddhhmmss"));
        }

        internal static void TestStringFormat(SPWeb web)
        {
            string s = "www{0}, {0} and {1}";
            Console.WriteLine(string.Format(s, "test1", "test2"));
        }

        internal static void TestContain(SPWeb web)
        {
            string applicant = @"Axel Kruse(CNAIDC\Axel.Kruse)";
            string user = @"CNAIDC\Axel.Kruse";

            Console.WriteLine(applicant.Contains(user));
        }

        internal static void TestContain()
        {
            stringList.Add("1");
            stringList.Add("2");
            foreach (string s in stringList)
            {
                stringList.Add("3");
                break;
            }

            TestContain();
        }

        private static void TestTransfer(string d)
        {
            stringList.Add(d);
        }
    }
}
