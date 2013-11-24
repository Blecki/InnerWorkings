using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assemblage
{
    public class TT3 : Hardware
    {
        public void PortWritten(byte port, byte value)
        {
            Console.Write((char)value);
        }

        public void Connect(IN8 CPU, params byte[] ports)
        {
            CPU.AttachHardware(this, ports);
        }

        public void Update()
        {

        }
    }
}
