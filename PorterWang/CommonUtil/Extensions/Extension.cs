using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PorterWang.CommonUtil
{
    public static class Extension
    {
        public static string AsString<T>(this T input) where T : class
        {
            return input == null ? string.Empty : input.ToString();
        }

        public static bool IsNotNullOrWhitespace(this string input)
        {
            return input != null && input.Trim().Length != 0;
        }
    }
}
