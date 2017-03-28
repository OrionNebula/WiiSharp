using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotrek.WiiSharp.Enum
{
    [Flags]
    internal enum WiimoteReportType : byte
    {
        ControlStatus = 0x20,
        Read = 0x21,
        Write = 0x22,

        BTN = 0x30,

        Accelerometer = 0x01,
        IR = 0x02,
        Accessory = 0x04,

    }
}
