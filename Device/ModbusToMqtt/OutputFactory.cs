using Newtonsoft.Json.Linq;
using Power21.Device.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Power21.Device
{
    public class OutputFactory
    {
        public static async Task<JObject> Output(IModbusInterface modbusInterface, JObject template, CancellationToken cancellationToken)
        {
            JObject cpyObj = template.DeepClone() as JObject;
            await UpdateToken(modbusInterface, cpyObj, cancellationToken);
            return cpyObj;
        }

        private static async Task ValidateAndBinding(JToken field, IModbusInterface modbusInterface, CancellationToken cancellationToken)
        {
            string fieldName = field.Value<string>();
            if (fieldName.StartsWith('$'))
            {
                string variable_name = fieldName.TrimStart('$');
                IComparable v = await modbusInterface.GetValue(variable_name, cancellationToken);
                field.Replace((dynamic)v);
            }
        }

        private static async Task UpdateToken(IModbusInterface modbusInterface, IEnumerable<JToken> obj, CancellationToken cancellationToken)
        {
            foreach (JToken token in obj)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                if (token.HasValues)
                    await UpdateToken(modbusInterface, token, cancellationToken);
                else
                {
                    string value = token.Value<string>();

                    // 우선 함수 항목인지 체크
                    if (OutputFunctionFactory.ValidateAndExecute(value, out string result))
                    {
                        token.Replace(result);
                    }
                    else
                        await ValidateAndBinding(token, modbusInterface, cancellationToken);
                }
            }
        }
    }
}
