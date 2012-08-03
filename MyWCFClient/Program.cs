using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyWCFClient
{
    class Program
    {
        static void Main(string[] args)
        {
            using (HelloWorldProxy proxy = new HelloWorldProxy())
            {
                Console.WriteLine(proxy.HelloWorld("WCF"));
                Console.Read();
            }
        }
    }
}
