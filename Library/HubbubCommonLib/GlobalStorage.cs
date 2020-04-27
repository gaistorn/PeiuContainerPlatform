using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform.Hubbub
{
    public interface IGlobalStorage
    {
        void SetValue(string Key, IComparable Value);
        Task<IComparable> GetValue(string key, CancellationToken cancellationToken);
        Task<JObject> BindingAndCopy(JObject obj, CancellationToken cancellationToken);
    }

    public class GlobalStorage : IGlobalStorage
    {
        private ConcurrentDictionary<string, IComparable> _valueMaps =
            new ConcurrentDictionary<string, IComparable>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public GlobalStorage()
        {
            //var factory = new ModbusFactory();
            //modbusMaster = factory.CreateMaster()
        }

        public async Task<JObject> BindingAndCopy(JObject template, CancellationToken cancellationToken)
        {
            JObject cpyObj = template.DeepClone() as JObject;
            if(cpyObj.ContainsKey("timestamp") == false)
            {
                string timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                cpyObj.Add("timestamp", timestamp);
                //cpyObj.Add()
            }
            await UpdateToken(cpyObj, cancellationToken);
            return cpyObj;
        }

        public async Task<IComparable> GetValue(string key, CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            return _valueMaps.GetValueOrDefault(key);
        }

        public void SetValue(string Key, IComparable Value)
        {
            _valueMaps[Key] = Value;
            _signal.Release();
        }

        private async Task ValidateAndBinding(JToken field, CancellationToken cancellationToken)
        {
            string fieldName = field.Value<string>();
            if (fieldName.StartsWith('$'))
            {
                string variable_name = fieldName.TrimStart('$');
                IComparable v = await this.GetValue(variable_name, cancellationToken);
                field.Replace((dynamic)v);
            }
        }

        private async Task UpdateToken(IEnumerable<JToken> obj, CancellationToken cancellationToken)
        {
            foreach (JToken token in obj)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                if (token.HasValues)
                    await UpdateToken(token, cancellationToken);
                else
                {
                    string value = token.Value<string>();
                    await ValidateAndBinding(token, cancellationToken);
                }
            }
        }
    }
}

