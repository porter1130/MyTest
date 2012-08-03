using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace MyWCFService
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        String HelloWorld(string name);
    }
}
