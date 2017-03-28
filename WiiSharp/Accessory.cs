using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lotrek.WiiSharp
{
    public abstract class Accessory
    {
        public Wiimote Wiimote
        {
            get;
            private set;
        }

        internal Accessory(Wiimote wiimote)
        {
            Wiimote = wiimote;
        }

        /// <summary>
        /// Complete the handshake with this accessory.
        /// </summary>
        /// <returns>Accessories that are children of this one.</returns>
        public abstract Accessory[] Handshake();
    }

    public class MotionPlus : Accessory
    {
        public byte[] ID
        {
            get;
            private set;
        }
        

        internal MotionPlus(Wiimote wiimote) : base(wiimote)
        {

        }

        public override Accessory[] Handshake()
        {
            if (!Wiimote.Connected)
                return null;

            Wiimote.State |= Enum.WiimoteState.AccessoryHandshake;
            Wiimote.Write(Enum.WiiMoteAddress.MPlusEnable, new byte[] { 0x04 });

            Thread.Sleep(500);



            return null;
        }
    }
}
