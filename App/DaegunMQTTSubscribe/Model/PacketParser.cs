using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PeiuPlatform.Models
{
    public static class PacketParser
    {
        public static float ConvertDecimalToIEE754float(byte[] bits)
        {
            byte[] fbits = new byte[4];
            byte[] primary_bits = new byte[2];
            byte[] secondry_bits = new byte[2];
            Array.Copy(bits, primary_bits, 2);
            Array.Copy(bits, 2, secondry_bits, 0, 2);

            Array.Copy(secondry_bits, 0, fbits, 0, 2);
            Array.Copy(primary_bits, 0, fbits, 2, 2);

            float fValue = BitConverter.ToSingle(fbits);
            return fValue;
        }


        public static T ByteToStruct<T>(byte[] buffer) where T : struct
        {
            int iSize = Marshal.SizeOf(typeof(T));

            if (iSize > buffer.Length)
            {
                throw new Exception(string.Format("SIZE ERR(len:{0} sz:{1})", buffer.Length, iSize));
            }

            IntPtr ptr = Marshal.AllocHGlobal(iSize);
            Marshal.Copy(buffer, 0, ptr, iSize);
            T obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);

            return obj;
        }
    }
}
