using LiteDB;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace PeiuPlatform.Lib
{
    public class EventSchema
    {
        public const string IdProperty = "_id";
        public const string FactoryCodeProperty = "FactoryCode";
        public const string GroupCodeProperty = "GroupCode";
        public const string DeviceIndexProperty = "DeviceIndex";
        public const string DeviceTypeProperty = "DeviceType";
        public const string ValueProperty = "Value";
        public long Id { get; set; }
        public int FactoryCode { get; set; }
        public int GroupCode { get; set; }

        public int DeviceIndex { get; set; }
        public int DeviceType { get; set; }
        public ushort Value { get; set; }

        public BsonDocument GetBsonDocument()
        {
            BsonDocument bdoc = new BsonDocument();
            bdoc.Add(IdProperty, Id);
            bdoc.Add(FactoryCodeProperty, FactoryCode);
            bdoc.Add(GroupCodeProperty, GroupCode);
            bdoc.Add(DeviceIndexProperty, DeviceIndex);
            bdoc.Add(DeviceTypeProperty, DeviceType);
            bdoc.Add(ValueProperty, (int)Value);
            return bdoc;
        }

        public static BsonDocument CreateEvent(long Id, int FactoryCode, int GroupCode, int DeviceIndex, int DeviceType, ushort Value)
        {
            BsonDocument bdoc = new BsonDocument();
            bdoc.Add(IdProperty, Id);
            bdoc.Add(FactoryCodeProperty, FactoryCode);
            bdoc.Add(GroupCodeProperty, GroupCode);
            bdoc.Add(DeviceIndexProperty, DeviceIndex);
            bdoc.Add(DeviceTypeProperty, DeviceType);
            bdoc.Add(ValueProperty, (int)Value);
            return bdoc;
        }
       
    }

    //public static class BsonDocumentExtension
    //{
    //    public static T Convert<T>(this BsonDocument document) where T : class, new()
    //    {
    //        T instance = new T();
    //        var pros = typeof(T).GetProperties();
    //        foreach(KeyValuePair<string, BsonValue>  row in document)
    //        {
    //            PropertyInfo pi = = com
    //        }
    //    }
    //}
}
