using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp.Extensions
{
    class MyComparer
    {
        #region Inner class
        public class MyCaseInsensitiveComparer : IEqualityComparer<string>
        {

            public bool Equals(string x, string y)
            {
                return x.Equals(y, StringComparison.CurrentCultureIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                throw new NotImplementedException();
            }
        }
        #endregion       
    }
}
