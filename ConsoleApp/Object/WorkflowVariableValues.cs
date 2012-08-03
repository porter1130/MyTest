using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ConsoleApp.Module;

namespace ConsoleApp.Object
{
    [Serializable, XmlRoot("Data")]
    public class WorkflowVariableValues : XMLEntry.SerializableDictionary<string, object>
    {
        public WorkflowVariableValues() { }
    }
}
