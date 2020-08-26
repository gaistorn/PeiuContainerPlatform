using System;
using System.Collections.Generic;
using System.Text;

namespace Hubbub
{
    partial class VwModbusInputPoint
    {
        private IComparable _value;
        public virtual IComparable GetValue() => _value;
        public virtual void SetValue(IComparable value) => _value = value;

        public virtual int GetGroupCode() => int.Parse($"{Hubbubid}{Deviceindex}{Functioncode}{Offset}");
    }
}
