using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CA
{
    [Serializable,XmlRoot("Data")]
    public class WorkflowVarableValues:SerializableDictionary<string,object>
    {
    }
}
