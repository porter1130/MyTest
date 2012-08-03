using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using MyWCFService;

namespace MyWCFClient
{
    class HelloWorldProxy : ClientBase<IService>, IService
    {
        public static readonly Binding HelloWorldBinding = new NetNamedPipeBinding();

        public static readonly EndpointAddress HelloWorldAddress = new EndpointAddress(new Uri("net.pipe://localhost/HelloWorld"));
        public HelloWorldProxy() : base(HelloWorldBinding, HelloWorldAddress) { }

        public string HelloWorld(string name)
        {
            return Channel.HelloWorld(name);
        }
    }

}
