using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyWCFService
{
    class Program
    {
        static void Main(string[] args)
        {
            using (MyServiceHost host = new MyServiceHost())
            {
                host.Open();
                Console.Read();
            }
        }
    }
}
