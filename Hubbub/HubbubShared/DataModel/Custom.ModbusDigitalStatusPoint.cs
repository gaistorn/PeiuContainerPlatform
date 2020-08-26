using System;
using System.Collections.Generic;
using System.Text;
namespace Hubbub
{

    /// <summary>
    /// There are no comments for Hubbub.ModbusDigitalStatusPoint, HubhubSharedLib in the schema.
    /// </summary>
    public partial class ModbusDigitalStatusPoint
    {
        public virtual int GetGroupCode() => int.Parse($"{Hubbubid}{Deviceindex}{Functioncode}{Offset}");
    }
}
