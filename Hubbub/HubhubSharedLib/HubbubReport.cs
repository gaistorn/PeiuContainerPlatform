using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Hubbub
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HubbubReport
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
        public string strHost;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
        public string strIp;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string strOSVer;

        public int iDeviceIndex;
        public int iSiteId;
        public enStatus Status;

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
            catch 
            {
                // 예외 발생
                throw;
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

    public enum enStatus : int
    {
        /// <summary>
        /// 정상동작중
        /// </summary>
        RUN,
        /// <summary>
        /// 사용자에 의한 일시 중지
        /// </summary>
        SUSPENDING, 
        /// <summary>
        /// 특정 원인에 의한 중단
        /// </summary>
        STOP,
        /// <summary>
        /// 공사중 또는 특정 작업에 의한 일시 중지
        /// </summary>
        UNDER_CONSTRUCTION,

        /// <summary>
        /// 오류 발생
        /// </summary>
        ERROR,

        /// <summary>
        /// 동작준비중
        /// </summary>
        INITIALIZE,

        /// <summary>
        /// 기본 상태
        /// </summary>
        NO_STATUS
    }
}
