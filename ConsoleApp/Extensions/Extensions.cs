using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

namespace ConsoleApp.Extensions
{
    static class Extensions
    {
       
        public static DataTable AsDataTable<T>(this IEnumerable<T> enumerable)
        {
            DataTable dt = new DataTable("Generated");

            T first = enumerable.FirstOrDefault();
            if (first == null)
            {
                return dt;  
            }

            PropertyInfo[] properties = first.GetType().GetProperties();

            foreach (PropertyInfo pi in properties)
            {
                dt.Columns.Add(pi.Name, pi.PropertyType);
            }

            foreach (T t in enumerable)
            {
                DataRow row = dt.NewRow();

                foreach (PropertyInfo pi in properties)
                {
                    row[pi.Name] = t.GetType().InvokeMember(pi.Name, BindingFlags.GetProperty, null, t, null);
                }

                dt.Rows.Add(row);
            }

            return dt;
        }

        public static string AsString<T>(this T input) where T : class
        {
            return input == null ? string.Empty : input.ToString();
        }

        public static bool IsNotNullOrWhitespace(this string input) {
            return input != null && input.Trim().Length != 0;
        }
    }
}
