using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotrek.WiiSharp.Enum
{
    internal enum WiimoteCommandType : byte
    {
        LED = 0x11,
        ReportType = 0x12,
        Rumble = 0x13,
        IR = 0x13,
        ControlStatus = 0x15,
        WriteData = 0x16,
        ReadData = 0x17,
        IR2 = 0x1A
    }
}
