using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPIEFolder.CMD
{
    class ExportCommand:Command
    {
        public ExportCommand(Receiver receiver) : base(receiver) { }

        public override void Execute()
        {
            receiver.Action();
        }
    }
}
