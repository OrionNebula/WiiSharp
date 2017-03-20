using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EnarcLabs.WiiSharp.Native
{
    internal static class HID
    {
        [DllImport("hid.dll", EntryPoint="HidD_GetHidGuid", SetLastError=true)]
        internal static extern void HidD_GetHidGuid(out Guid Guid);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SetupDiGetClassDevs(
            ref Guid ClassGuid,
            [MarshalAs(UnmanagedType.LPTStr)] string Enumerator,
            IntPtr hwndParent,
            uint Flags
        );

        [DllImport(@"setupapi.dll", CharSet=CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInterfaces(
            IntPtr hDevInfo,
            IntPtr devInfo,
            ref Guid interfaceClassGuid,
            uint memberIndex,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData
        );

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(
          IntPtr hDevInfo,
          ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
          ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
          uint deviceInterfaceDetailDataSize,
          ref uint requiredSize,
          ref SP_DEVINFO_DATA deviceInfoData
        );

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(
          IntPtr hDevInfo,
          ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
          ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
          uint deviceInterfaceDetailDataSize,
          IntPtr requiredSize,
          IntPtr deviceInfoData
        );

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(
          IntPtr hDevInfo,
          ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
          IntPtr deviceInterfaceDetailData,
          uint deviceInterfaceDetailDataSize,
          ref uint requiredSize,
          IntPtr deviceInfoData
        );

        [DllImport("hid.dll", SetLastError=true)]
        public static extern Boolean HidD_GetAttributes(IntPtr DeviceObject, ref HIDD_ATTRIBUTES Attributes );

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_SetOutputReport(
                 IntPtr HidDeviceObject,
                 byte[] lpReportBuffer,
                 int ReportBufferLength);

    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HIDD_ATTRIBUTES
    {
        public Int32 Size;
        public Int16 VendorID;
        public Int16 ProductID;
        public Int16 VersionNumber;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
    {
        public int cbSize;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string DevicePath;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SP_DEVINFO_DATA
    {
        public uint cbSize;
        public Guid classGuid;
        public uint devInst;
        public IntPtr reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SP_DEVICE_INTERFACE_DATA
    {
        public Int32 cbSize;
        public Guid interfaceClassGuid;
        public Int32 flags;
        private UIntPtr reserved;
    }

}
