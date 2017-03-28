using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotrek.WiiSharp.Enum
{
    internal enum WiiMoteAddress : uint
    {
        OffsetCalibration = 0x16,
        Base = 0x4A40000,
        ID = Base + 0xFA,
        Enable = Base + 0x40,
        Enable1 = Base + 0xF0,
        Enable2 = Base + 0xFB,
        Calibrate = Base + 0x20,
        MPlusIdent = Base + 0x200FA,
        MPlusEnable = Base + 0x200FE,
        MPlusInit = Base + 0x200F0,
        IR = 0x4B00030,
        IRBlock1 = 0x4B00000,
        IRBlock2 = 0x4B0001A,
        IRModeNum = 0x4B00033
    }
}