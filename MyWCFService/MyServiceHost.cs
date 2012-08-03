using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace MyWCFService
{
    class MyServiceHost : IDisposable
    {
        private ServiceHost _myHost;
        public const String BaseAddress = "net.pipe://localhost";
        public const string HelloWorldServiceAddress = "HelloWorld";

        public static readonly Type ContractType = typeof(IService);
        public static readonly Type ServiceType = typeof(HelloWorldService);

        public static readonly Binding HelloWorldBinding = new NetNamedPipeBinding();

        protected void ConstructServiceHost()
        {
            _myHost = new ServiceHost(ServiceType, new Uri[] { new Uri(BaseAddress) });
            _myHost.AddServiceEndpoint(ContractType, HelloWorldBinding, HelloWorldServiceAddress);
        }

        public ServiceHost Host {
            get { return _myHost; }
        }

        public void Open() {
            Console.WriteLine("Start Service...");
            _myHost.Open();
            Console.WriteLine("Service has started...");
        }

        public MyServiceHost() {
            ConstructServiceHost();
        }
        public void Dispose()
        {
            if (_myHost != null) {
                (_myHost as IDisposable).Dispose();
            }
        }
    }
}
