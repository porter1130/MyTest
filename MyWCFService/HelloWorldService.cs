using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyWCFService
{
    class HelloWorldService:IService
    {
        public string HelloWorld(string name)
        {
            return name + " say: HelloWorld!";
        }
    }
}
