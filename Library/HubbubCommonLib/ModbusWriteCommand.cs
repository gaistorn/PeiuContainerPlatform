﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Hubbub
{
    public class ModbusWriteCommand
    {
        public ushort StartAddress { get; set; }
        public ushort WriteValue { get; set; }

        public int FunctionCode { get; set; } = 3;
    }
}
