using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Hubbub
{
    public class DataPoint
    {
        public string Category { get; set; }
        public ushort Address { get; set; }
        public int WordSize { get; set; }
        public string Name { get; set; }
        public int Digit { get; set; }
        public bool IsDigit
        {
            get
            {
                return Digit != -1;
            }
        }
        public string Description { get; set; }
        public DataType Type { get; set; }
        public float Scale { get; set; }

        public override string ToString()
        {
            return $"[{Category}] {Address} {Name}";
        }

        public string GetUniqueId()
        {
            return $"{Category}.{Name}";
        }
    }

    public enum DataType
    {
        SGL,
        U32,
        I32,
        U16,
        I16
    }
}
