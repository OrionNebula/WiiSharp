using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnarcLabs.WiiSharp.Enum
{
    [Flags]
    public enum WiimoteState : int
    {
        DeviceFound         = 0x1,
        Handshake           = 0x2,
        HandshakeComplete   = 0x4,
        Connected           = 0x8,
        Rumble              = 0x10,
        Accelerometer       = 0x20,
        Accessory           = 0x40,
        IR                  = 0x80,
        Speaker             = 0x100,
        IRSensorLevel1      = 0x200,
        IRSensorLevel2      = 0x400,
        IRSensorLevel3      = 0x800,
        IRSensorLevel4      = 0x1000,
        IRSensorLevel5      = 0x2000,

        AccessoryHandshake  = 0x4000,
        AccessoryExternal   = 0x8000,
        AccessoryFailed     = 0x10000,
        MPlusPresent        = 0x80000
    }
}
