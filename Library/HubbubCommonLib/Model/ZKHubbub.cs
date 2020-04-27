using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PeiuPlatform.Hubbub.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZKHubbub
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
        public string strHost;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
        public string strIp;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string strOSVer;

        public long lShutonUtcUnixTimeSeconds;

        public byte[] ToByteArray()
        {

            byte[] arr = null;
            IntPtr ptr = IntPtr.Zero;
            try
            {
                Int16 size = (Int16)Marshal.SizeOf(this);
                arr = new byte[size];
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(this, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);

            }
            catch (Exception e)
            {
                // 예외 발생
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return arr;
        }

        public int GetSize()
        {
            return Marshal.SizeOf(this);
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZKDevice
    {
        public long lLastOperateUtcUnixtimeSeconds;

        public byte[] ToByteArray()
        {

            byte[] arr = null;
            IntPtr ptr = IntPtr.Zero;
            try
            {
                Int16 size = (Int16)Marshal.SizeOf(this);
                arr = new byte[size];
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(this, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);

            }
            catch (Exception e)
            {
                // 예외 발생
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return arr;
        }

        public int GetSize()
        {
            return Marshal.SizeOf(this);
        }
    }

}
