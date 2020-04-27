using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PES.Service.DataService
{
    public static class LogEventMaker
    {
        public static NLog.LogEventInfo CreateLogEvent(string loggerName, object target)
        {
            var logEvent = new NLog.LogEventInfo(NLog.LogLevel.Trace, loggerName, "");
            var properties = target.GetType().GetFields();

            ExportFields(logEvent.Properties, null, target);
            return logEvent;
        }

        public static void ExportFields(IDictionary<object, object> propertiesMap, string aliasName, object target)
        {

            var properties = target.GetType().GetFields();
            string attachName = string.IsNullOrEmpty(aliasName) ? "" : aliasName + "_";
            foreach (FieldInfo field in properties)
            {
                Type fieldType = field.FieldType;
                object fieldValue = field.GetValue(target);
                if (fieldValue == null)
                    continue;
                        
                string fieldName = attachName + field.Name;
                if (fieldType.IsArray)
                {
                    int idx = 0;
                    Array arValue = (Array)fieldValue;
                    System.Collections.IList sampleObject_test1 = (System.Collections.IList)field.GetValue(target);
                    // We can now get the first element of the array of Test2s:
                    foreach (object ar_value in arValue)
                    {
                        propertiesMap[$"{fieldName}{idx++}"] = ar_value;
                    }
                }
                else if (fieldType.IsValueType && !fieldType.IsPrimitive && !fieldType.IsEnum)
                {
                    // Struct
                    ExportFields(propertiesMap, fieldName, fieldValue);
                }
                else
                {
                    propertiesMap[fieldName] = fieldValue;
                }
            }
        }
    }
}
