using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assemblage
{
    public interface Hardware
    {
        void Connect(IN8 CPU, params byte[] ports);
        void PortWritten(byte port, byte value);
        void Update();
    }
}
