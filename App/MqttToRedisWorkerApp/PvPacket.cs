using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MqttToRedisWorkerApp
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PvPacket
    {
        public ushort deviceindex;
        public ushort siteid;
        public ushort rcc;
        public ushort injeju;
        public float totalactivepower;
        public float totalreactivepower;
        public float reverseactivepower;
        public float reversereactivepower;
        public rst voltage;
        public rst current;
        public float frequency;
        public float energytotalactivepower;
        public float energytodayactivepower;
        public float energytotalreactivepower;
        public float energytotalreverseactivepower;

        public int GetSize()
        {
            return Marshal.SizeOf(this);
        }

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
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct rst
    {
        public float r;
        public float s;
        public float t;
    }
}
