namespace PeiuPlatform.Hubbub
{
    public class PcsOperationStatus
    {
        public const ushort RunStop = 2;
        public const ushort StandBy = 4;
        public const ushort Fault = 8;
        public const ushort Charge = 32;
        public const ushort Discharge = 64;
        public const ushort AC_CB = 128;
        public const ushort Warning = 1024;
        public const ushort AC_MC = 2048;
        public const ushort DC_CB = 4096;
    }
}