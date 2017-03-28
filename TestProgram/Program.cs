using Lotrek.WiiSharp;
using Lotrek.WiiSharp.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TestProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting the process:");
            var wii = Wiimote.FindWiiMotes().Where(x => x.Active).ToArray();
            Console.WriteLine("Found {0} remotes", wii.Length);

            int i = 1;
            foreach (var mote in wii)
            {
                mote.Leds = (WiimoteLeds)i++;
                //mote.Features = WiimoteFeatures.Accelerometer;
                mote.OnDisconnect += Mote_OnDisconnect;
                mote.OnButtonPressed += Mote_OnButtonPressed;
                mote.OnAccelerometerData += Mote_OnAccelerometerData;
            }

            new Thread(() => {
                while (true)
                    foreach (var mote in wii)
                    {
                        mote.Poll(TimeSpan.FromMilliseconds(1000));
                    }
            }) { Name = "Wiimote Polling Thread" }.Start();

            Console.Clear();
        }

        private static void Mote_OnAccelerometerData(Wiimote remote, Vector3 accel, Vector2 rotation)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("Found this out: {{ x: {0:000}, y: {1:000}, z: {2:000} }}            ", accel.X - 127, accel.Y - 127, accel.Z - 127);
        }

        private static void Mote_OnButtonPressed(Wiimote remote, WiimoteButton buttons, WiimoteButton risingEdge, WiimoteButton fallingEdge)
        {
            Console.SetCursorPosition(0, 0);

            Console.ForegroundColor = buttons.HasFlag(WiimoteButton.Up) ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(" ^ ");
            Console.ForegroundColor = buttons.HasFlag(WiimoteButton.Left) ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write("<");
            Console.ForegroundColor = buttons.HasFlag(WiimoteButton.Right) ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(" >");
            Console.ForegroundColor = buttons.HasFlag(WiimoteButton.Down) ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(" v ");
            Console.ForegroundColor = buttons.HasFlag(WiimoteButton.B) ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write("B");
            Console.ForegroundColor = buttons.HasFlag(WiimoteButton.A) ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(" A");
            Console.ForegroundColor = buttons.HasFlag(WiimoteButton.Minus) ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write("-");
            Console.ForegroundColor = buttons.HasFlag(WiimoteButton.Home) ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write("H");
            Console.ForegroundColor = buttons.HasFlag(WiimoteButton.Plus) ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine("+");
            Console.ForegroundColor = buttons.HasFlag(WiimoteButton.One) ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write("1");
            Console.ForegroundColor = buttons.HasFlag(WiimoteButton.Two) ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(" 2");
        }

        private static void Mote_OnDisconnect(Wiimote remote)
        {
            Console.WriteLine("A remote has disconnected");
        }
    }
}
