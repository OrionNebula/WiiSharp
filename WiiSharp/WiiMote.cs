using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EnarcLabs.WiiSharp.Enum;
using EnarcLabs.WiiSharp.Native;

namespace EnarcLabs.WiiSharp
{
    /// <summary>
    /// Called when a Wii remote is disconnected.
    /// </summary>
    /// <param name="remote">The Wii remote instance that was disconnected.</param>
    public delegate void WiimoteDisconnected(Wiimote remote);
    /// <summary>
    /// Called when button data reaches the Wii remote.
    /// </summary>
    /// <param name="remote">The Wii remote instance that generated the event.</param>
    /// <param name="buttons">A bitmask of all buttons currently pressed.</param>
    /// <param name="leadingEdge">A bitmask of the buttons newly pressed on this frame.</param>
    /// <param name="fallingEdge">A bitmask of the buttons newly released on this frame.</param>
    public delegate void WiimoteButtonPressed(Wiimote remote, WiimoteButton buttons, WiimoteButton leadingEdge, WiimoteButton fallingEdge);
    /// <summary>
    /// Called when accelerometer data reaches the Wii remote.
    /// </summary>
    /// <param name="remote">The Wii remote instance that generated the event.</param>
    /// <param name="accel">Raw accelerometer data from the remote.</param>
    /// <param name="rotation">The roll and pitch of the Wii remote.</param>
    public delegate void WiimoteAccelerometerData(Wiimote remote, Vector3 accel, Vector2 rotation);

    /// <summary>
    /// Represents a Wii remote.
    /// </summary>
    public class Wiimote : IDisposable
    {
        private WiimoteButton _LastButtons;
        private IntPtr _Handle;
        private ManualResetEvent _Event;
        private NativeOverlapped _Overlap;

        #region Properties

        /// <summary>
        /// An identifier unique to this Wii remote. This does not persist between sessions.
        /// </summary>
        public Guid WiiMoteID
        {
            get;
            private set;
        }

        /// <summary>
        /// Identifies the type of Wii remote being accessed.
        /// </summary>
        public WiimoteType Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Identifies the state of this Wii remote.
        /// </summary>
        public WiimoteState State
        {
            get;
            internal set;
        }

        /// <summary>
        /// Holds certain internal attributes of the Wii remote.
        /// </summary>
        public WiimoteFlags Flags
        {
            get;
            private set;
        }

        /// <summary>
        /// Identifies the stack used to interface with the Wii remote.
        /// </summary>
        public WiimoteStack Stack
        {
            get;
            private set;
        }

        /// <summary>
        /// A set of the accessories connected to the current Wii remote.
        /// </summary>
        public Accessory[] Accessories
        {
            get;
            private set;
        }

        private WiimoteLeds _Leds;
        /// <summary>
        /// Gets or sets the LEDs currently lit.
        /// </summary>
        public WiimoteLeds Leds
        {
            get
            {
                return _Leds;
            }
            set
            {
                if (!Connected)
                    return;

                Send(WiimoteCommandType.LED, new byte[] { (byte)((int)value << 4 | (Rumble ? 0x1 : 0)) });
                _Leds = value;
            }
        }

        /// <summary>
        /// Gets or sets the activation state of the rumble motor.
        /// </summary>
        public bool Rumble
        {
            get
            {
                return State.HasFlag(WiimoteState.Rumble);
            }
            set
            {
                if (!Connected)
                    return;

                if (value == Rumble)
                    return;

                if (value)
                    State |= WiimoteState.Rumble;
                else
                    State &= ~WiimoteState.Rumble;

                Send(WiimoteCommandType.Rumble, new byte[] { (byte)((int)_Leds << 4 | (Rumble ? 0x1 : 0)) });
            }
        }

        private WiimoteFeatures _Features;
        /// <summary>
        /// Describes the features enabled about this Wii remote.
        /// </summary>
        public WiimoteFeatures Features
        {
            get
            {
                return _Features;
            }
            set
            {
                if (value.HasFlag(WiimoteFeatures.Accelerometer))
                    State |= WiimoteState.Accelerometer;
                else
                    State &= ~WiimoteState.Accelerometer;

                if (value.HasFlag(WiimoteFeatures.Accessory))
                {
                    State |= WiimoteState.Accessory;
                    State &= ~(WiimoteState.AccessoryExternal | WiimoteState.AccessoryFailed | WiimoteState.AccessoryHandshake);
                    //TODO: Accessory handshake
                }
                else
                {
                    State &= ~(WiimoteState.Accessory | WiimoteState.AccessoryExternal | WiimoteState.AccessoryFailed | WiimoteState.AccessoryHandshake);
                    //TODO: Accessory disconnect
                }

                if (value.HasFlag(WiimoteFeatures.IR))
                    State |= WiimoteState.IR;
                else
                    State &= ~WiimoteState.IR;

                _Features = value;
                SetReportType();
            }
        }

        /// <summary>
        /// True if the Wii remote is connected. Note that this does not mean a handshake has succeeded.
        /// </summary>
        public bool Connected
        {
            get
            {
                return State.HasFlag(WiimoteState.Connected);
            }
        }

        /// <summary>
        /// True if the Wii remote has completed a handshake.
        /// </summary>
        public bool Active
        {
            get
            {
                return State.HasFlag(WiimoteState.HandshakeComplete);
            }
        }

        #endregion

        internal Wiimote(IntPtr handle, WiimoteType type)
        {
            _Handle = handle;
            Type = type;
            WiiMoteID = Guid.NewGuid();
        }

        /// <summary>
        /// Look for new data from this Wii remote and invoke event handlers.
        /// </summary>
        public void Poll(TimeSpan timeout)
        {
            var dat = new byte[32];

            if(Recieve(dat, timeout))
            {
                var type = (WiimoteReportType)dat[0];

                if(type.HasFlag(WiimoteReportType.BTN))
                {
                    var btns = new byte[2];
                    Array.Copy(dat, 1, btns, 0, 2);
                    var mask = (WiimoteButton)(BitConverter.ToUInt16(btns.Reverse().ToArray(), 0) & (ushort)WiimoteButton.All);
                    OnButtonPressed?.Invoke(this, mask, mask & ~_LastButtons, _LastButtons & ~mask);
                    _LastButtons = mask;

                }

                if(type.HasFlag(WiimoteReportType.Accelerometer))
                {
                    var accel = new Vector3(dat[3], dat[4], dat[5]);
                    OnAccelerometerData?.Invoke(this, accel, new Vector2(0, 0));
                }

                if(type.HasFlag(WiimoteReportType.Accessory))
                {

                }
            }else
            {
                //TODO: Send waiting writes
            }
        }

        /// <summary>
        /// Close your connection to this Wii remote.
        /// </summary>
        public void Disconnect()
        {
            if (!Connected)
                return;

            Leds = WiimoteLeds.None;
            Rumble = false;

            Kernel.CloseHandle(_Handle);
            _Handle = IntPtr.Zero;

            _Event.Reset();
            //Kernel.ResetEvent(_Overlap.EventHandle);

            State &= ~(WiimoteState.Connected | WiimoteState.Handshake | WiimoteState.HandshakeComplete);

            OnDisconnect?.Invoke(this);
        }

        public void Dispose()
        {
            Disconnect();
        }

        #region Internal Methods

        private bool SetReportType()
        {
            if (!State.HasFlag(WiimoteState.Connected))
                return false;

            var buf = new byte[]
            {
                (byte)((Flags.HasFlag(WiimoteFlags.Continuous) ? 0x04 : 0x00) | (Rumble ? 0x01 : 0x00)),
                0x00
            };

            var motion = State.HasFlag(WiimoteState.Accelerometer);
            var acc = State.HasFlag(WiimoteState.Accessory);
            var ir = State.HasFlag(WiimoteState.IR);

            buf[1] = (byte)( WiimoteReportType.BTN | (motion ? WiimoteReportType.Accelerometer : 0) | (acc ? WiimoteReportType.Accessory : 0) | (ir ? WiimoteReportType.IR : 0) );

            if (!Send(WiimoteCommandType.ReportType, buf))
                return false;

            return buf[1] != 0;
        }

        internal bool Send(WiimoteCommandType type, byte[] msg)
        {
            switch (type)
            {
                case WiimoteCommandType.LED:
                case WiimoteCommandType.Rumble:
                case WiimoteCommandType.ControlStatus:
                    {
                        if (Rumble)
                            msg[0] |= 0x01;

                        break;
                    }
                default:
                    break;
            }

            if (!State.HasFlag(WiimoteState.Connected))
                return false;

            var buf = new byte[32];
            buf[0] = (byte)type;
            msg.CopyTo(buf, 1);

            uint bytes = 0;
            switch (Stack)
            {
                case WiimoteStack.Unknown:
                    {
                        if (Kernel.WriteFile(_Handle, buf, 22, out bytes, ref _Overlap))
                        {
                            Stack = WiimoteStack.BlueSoleil;
                            return true;
                        }

                        if(HID.HidD_SetOutputReport(_Handle, buf, msg.Length + 1))
                        {
                            Stack = WiimoteStack.MS;
                            return true;
                        }

                        return false;
                    }
                case WiimoteStack.MS:
                    return HID.HidD_SetOutputReport(_Handle, buf, msg.Length + 1);
                case WiimoteStack.BlueSoleil:
                    return Kernel.WriteFile(_Handle, buf, 22, out bytes, ref _Overlap);
            }

            return false;
        }

        internal bool Write(WiiMoteAddress addr, byte[] data)
        {
            if (!Connected || (data?.Length ?? 0) == 0)
                return false;

            var buf = new byte[21];
            var bAddr = BitConverter.GetBytes((uint)addr);
            if(BitConverter.IsLittleEndian)
                bAddr = bAddr.Reverse().ToArray();

            bAddr.CopyTo(buf, 0);

            var bLen = BitConverter.GetBytes((byte)data.Length);
            if (BitConverter.IsLittleEndian)
                bLen = bLen.Reverse().ToArray();

            bLen.CopyTo(buf, 0);

            Send(WiimoteCommandType.WriteData, buf);

            return true;
        }

        internal bool Recieve(byte[] buf, TimeSpan timeout)
        {
            if (!State.HasFlag(WiimoteState.Connected))
                return false;

            uint read;
            if(!Kernel.ReadFile(_Handle, buf, (uint)buf.Length, out read, ref _Overlap))
            {
                var b = Marshal.GetLastWin32Error();

                if(b == 38 || b == 1167)
                {
                    OnDisconnect?.Invoke(this);
                    return false;
                }

                _Event.WaitOne(timeout);
            }

            uint bytes;
            if (!Kernel.GetOverlappedResult(_Handle, ref _Overlap, out bytes, false))
                return false;

            _Event.Reset();
            //Kernel.ResetEvent(_Overlap.EventHandle);
            return true;
        }

        internal bool Read(bool memory, WiiMoteAddress addr, byte[] buf, int readCt)
        {
            return true;
        }

        internal void Status()
        {
            if (!State.HasFlag(WiimoteState.Connected))
                return;

            Send(WiimoteCommandType.ControlStatus, new byte[1]);
        }

        internal bool WaitForReport(WiimoteReportType type, byte[] buf, TimeSpan timeout)
        {
            var start = DateTime.Now;
            while(true)
            {
                if(Recieve(buf, timeout))
                    if(buf[0] == (byte)type)
                        return true;
                    else if (buf[0] != 0x30)
                    {
                        //TODO: Handle report drop
                    }

                if (start + timeout > DateTime.Now)
                    return false;
            }
        }

        internal void Handshake(byte[] data = null)
        {
            var buf = new byte[32];

            //Reset wiimote

            State |= WiimoteState.Connected | WiimoteState.Handshake;
            State &= ~(WiimoteState.Accelerometer | WiimoteState.IR | WiimoteState.Rumble | WiimoteState.Accessory);
            Flags &= ~WiimoteFlags.Continuous;

            SetReportType();
            Thread.Sleep(500);

            Write(WiiMoteAddress.Enable1, new byte[] { 0x55 });

            //calibrate accelerometers

            Read(true, WiiMoteAddress.OffsetCalibration, buf, 8);


            var calZero = new Tuple<byte, byte, byte>(buf[0], buf[1], buf[2]);
            var calG = new Tuple<byte, byte, byte>((byte)(buf[4] - calZero.Item1), (byte)(buf[5] - calZero.Item2), (byte)(buf[6] - calZero.Item3));

            //re-enable IR and ask for status

            State |= WiimoteState.HandshakeComplete;
            State &= ~WiimoteState.Handshake;

            if (State.HasFlag(WiimoteState.IR))
            {
                State &= ~WiimoteState.IR;
                //wiiuse_set_ir(1)
            }

            for (int i = 0; i < 3; i++)
            {
                Status();
                WaitForReport(WiimoteReportType.ControlStatus, buf, Timeout.InfiniteTimeSpan);

                if (buf[3] != 0)
                    break;

                Thread.Sleep(500);
            }
        }

        #endregion

        public static Wiimote[] FindWiiMotes()
        {
            var toRet = new List<Wiimote>();

            Guid deviceId;
            HID.HidD_GetHidGuid(out deviceId); //Get the GUID for the HID class of devices

            var info = HID.SetupDiGetClassDevs(ref deviceId, null, IntPtr.Zero, 0x10 | 0x2); //Get a handle pointing to the list of HIDs

            SP_DEVICE_INTERFACE_DATA devData = default(SP_DEVICE_INTERFACE_DATA);
            devData.cbSize = Marshal.SizeOf(devData);
            for(uint i = 0; HID.SetupDiEnumDeviceInterfaces(info, IntPtr.Zero, ref deviceId, i, ref devData); i++) //Iterate over all HIDs
            {
                uint len = 0;
                HID.SetupDiGetDeviceInterfaceDetail(info, ref devData, IntPtr.Zero, 0, ref len, IntPtr.Zero); //Get the length of the data to grab later.

                SP_DEVICE_INTERFACE_DETAIL_DATA detailData = default(SP_DEVICE_INTERFACE_DETAIL_DATA);
                detailData.cbSize = 4 + Marshal.SystemDefaultCharSize; //Evil black magic. Trust me.
                if (!HID.SetupDiGetDeviceInterfaceDetail(info, ref devData, ref detailData, len, IntPtr.Zero, IntPtr.Zero)) //Get the device detail. Skip this device if it fails.
                    continue;

                //Get a handle pointing to this device.
                var dev = Kernel.CreateFile(detailData.DevicePath, EFileAccess.GenericRead | EFileAccess.GenericWrite, EFileShare.Read | EFileShare.Write, IntPtr.Zero, System.IO.FileMode.Open, EFileAttributes.Overlapped, IntPtr.Zero);

                //Get the device's attributes.
                HIDD_ATTRIBUTES attr = default(HIDD_ATTRIBUTES);
                attr.Size = Marshal.SizeOf(attr);
                HID.HidD_GetAttributes(dev, ref attr);

                //Make sure the device is a Wii remote.
                if (attr.VendorID == Constants.WMVendorID && (attr.ProductID == Constants.WMProductID || attr.ProductID == Constants.TRProductID))
                {
                    //Construct and populate the new WiiMote object
                    var wm = new Wiimote(dev, attr.ProductID == Constants.TRProductID ? WiimoteType.MotionPlusInside : WiimoteType.Regular)
                    {
                        State = WiimoteState.DeviceFound | WiimoteState.Connected
                    };

                    wm._Event = new ManualResetEvent(false);
                    wm._Overlap = new NativeOverlapped() { EventHandle = wm._Event.SafeWaitHandle.DangerousGetHandle() };

                    toRet.Add(wm);

                    if (!wm.SetReportType()) //Poke the remote to make sure it really is connected.
                    {
                        wm.State &= ~WiimoteState.Connected;
                        continue;
                    }

                    //Initiate the handshake process.
                    wm.Handshake();
                }
                else
                    Kernel.CloseHandle(dev); //This HID was not actually a Wii remote. Get rid of it.
                
            }

            return toRet.ToArray();
        }

        #region Events

        /// <summary>
        /// Called when the Wii remote is disconnected from the computer.
        /// </summary>
        public event WiimoteDisconnected OnDisconnect;

        /// <summary>
        /// Called when a button on the Wii remote changes state.
        /// </summary>
        public event WiimoteButtonPressed OnButtonPressed;

        /// <summary>
        /// Called when the Wii remote has new accelerometer data.
        /// </summary>
        public event WiimoteAccelerometerData OnAccelerometerData;

        #endregion
    }
}
