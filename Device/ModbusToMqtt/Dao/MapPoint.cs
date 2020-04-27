using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NModbus.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Power21.Device.Dao
{
    public class MapPoint
    {
        [JsonProperty("offset")]
        public ushort Offset { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("type")]
        public PointType PointType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("scale")]
        public float Scale { get; set; } = 0f;

        [JsonProperty("disable")]
        public bool Disable { get; set; } = false;

        public ushort[] ToBytesValue(IComparable value)
        {
            ushort[] buffers = null;
            switch (this.PointType)
            {
                case PointType.INT16:
                case PointType.UINT16:
                    buffers = new ushort[] { Convert.ToUInt16(value.ToString()) };
                    break;
                case PointType.INT32:
                    ushort lowOrderValue = BitConverter.ToUInt16(BitConverter.GetBytes((int)value), 2);
                    ushort highOrderValue = BitConverter.ToUInt16(BitConverter.GetBytes((int)value), 0);
                    buffers = new ushort[] { lowOrderValue, highOrderValue };
                    break;
                case PointType.UINT32:
                    lowOrderValue = BitConverter.ToUInt16(BitConverter.GetBytes((uint)value), 2);
                    highOrderValue = BitConverter.ToUInt16(BitConverter.GetBytes((uint)value), 0);
                    buffers = new ushort[] { lowOrderValue, highOrderValue };
                    break;
                case PointType.FLOAT:
                    lowOrderValue = BitConverter.ToUInt16(BitConverter.GetBytes((float)value), 2);
                    highOrderValue = BitConverter.ToUInt16(BitConverter.GetBytes((float)value), 0);
                    buffers = new ushort[] { lowOrderValue, highOrderValue };
                    break;
            }
            return buffers;
        }

        public IComparable ToValue(byte[] buffers)
        {
            bool IsStringValue = false;
            IComparable value = 0;

            // 혹시 스트링 값인지 체크
            string strValue = Encoding.ASCII.GetString(buffers);
            float fv = 0;
            IsStringValue = float.TryParse(strValue, out fv);

            switch (this.PointType)
            {
                case PointType.INT16:
                    if (IsStringValue)
                        value = Convert.ToInt16(strValue);
                    else
                        value = BitConverter.ToInt16(buffers);
                    break;
                case PointType.UINT16:
                    if (IsStringValue)
                        value = Convert.ToUInt16(strValue);
                    else
                        value = BitConverter.ToUInt16(buffers);
                    break;
                case PointType.INT32:
                    if (IsStringValue)
                        value = Convert.ToInt32(strValue);
                    else
                        value = BitConverter.ToInt32(buffers);
                    break;
                case PointType.UINT32:
                    if (IsStringValue)
                        value = Convert.ToUInt32(strValue);
                    else
                        value = BitConverter.ToUInt32(buffers);
                    break;
                case PointType.FLOAT:
                    if (IsStringValue == false)
                        value = BitConverter.ToSingle(buffers);
                    else
                        value = fv;
                    break;
            }

            if (Scale != 0)
                return (dynamic)value * Scale;

            return value;
        }

        public int GetSize()
        {
            switch(PointType)
            {
                case PointType.INT32:
                case PointType.FLOAT:
                case PointType.UINT32:
                    return 2;
                case PointType.INT64:
                    return 4;
                default:
                    return 1;
            }
        }
    }
}
