using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hubbub
{
    public interface IAsyncDataAccessor<TKey,TValue>
    {
        void SetValue(TKey Key, TValue Value);
        TValue GetValue(TKey key);
        bool ContainKey(TKey Key);
    }
    //public interface IGlobalStorage : IAsyncDataAccessor<T>
    //{
    //    Task<JObject> BindingAndCopy(JObject obj, CancellationToken cancellationToken);

    //    //Task<IEnumerable< ModbusWriteCommand>> GetWriteValues(CancellationToken cancellationToken);
    //    //void SetWriteValues(ModbusWriteCommand command);

    //    //Task<IEnumerable<EventModel>> GetEventModels(CancellationToken cancellationToken);
    //    //void SetEventValues(EventModel model);
    //}

    public class GlobalStorage<TKey,TValue> : IAsyncDataAccessor<TKey, TValue>
    {
        private ConcurrentDictionary<TKey, TValue> _valueMaps =
            new ConcurrentDictionary<TKey, TValue>();

        //private ConcurrentQueue<ModbusWriteCommand> _writeQueue = new ConcurrentQueue<ModbusWriteCommand>();

        //private ConcurrentQueue<EventModel> _eventModels = new ConcurrentQueue<EventModel>();

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
                string timestamp = DateTimeOffset.Now.ToString("yyyyMMddHHmmss");
                cpyObj.Add("timestamp", timestamp);
                timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                cpyObj.Add("utctimestamp", timestamp);
                //cpyObj.Add()
            }
            else
            {
                cpyObj["timestamp"] = DateTimeOffset.Now.ToString("yyyyMMddHHmmss");
                cpyObj["utctimestamp"] = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
            }
            await UpdateToken(cpyObj, cancellationToken);
            return cpyObj;
        }

        public bool ContainKey(TKey Key)
        {
            return _valueMaps.ContainsKey(Key);
        }

        public virtual TValue GetValue(TKey key)
        {
            return _valueMaps.GetValueOrDefault(key);
        }

        public virtual void SetValue(TKey Key, TValue Value)
        {
            _valueMaps.AddOrUpdate(Key, Value, (o, d) => { return d; });
        }

        //private async Task ValidateAndBinding(JToken field)
        //{
        //    string fieldName = field.Value<string>();
        //    //if (fieldName.StartsWith('$'))
        //    //{
        //    //    string variable_name = fieldName.TrimStart('$');
        //    //    if(variable_name == "PCS#3.ActivePower")
        //    //    {

        //    //    }
        //    //    T v = await this.GetValue(variable_name);
        //    //    field.Replace((dynamic)v);
        //    //}
        //}

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
                    //string value = token.Value<string>();
                    //await ValidateAndBinding(token);
                }
            }
        }

        //public async Task<IEnumerable<ModbusWriteCommand>> GetWriteValues(CancellationToken cancellationToken)
        //{
        //    await Semaphore.WaitAsync(cancellationToken);
        //    List<ModbusWriteCommand> commands = new List<ModbusWriteCommand>();
        //    while (_writeQueue.TryDequeue(out ModbusWriteCommand modbusWriteCommand))
        //        commands.Add(modbusWriteCommand);
        //    return commands;
        //}

        //public void SetWriteValues(ModbusWriteCommand command)
        //{
        //    _writeQueue.Enqueue(command);
        //    Semaphore.Release();
        //}

        //public async Task<IEnumerable<EventModel>> GetEventModels(CancellationToken cancellationToken)
        //{
        //    //await Semaphore.WaitAsync(cancellationToken);
        //    //List<EventModel> evtmodels = new List<EventModel>();
        //    //while (_eventModels.TryDequeue(out EventModel model))
        //    //    evtmodels.Add(model);
        //    //return evtmodels;
        //    return _eventModels;
        //}

        //public void SetEventValues(EventModel model)
        //{
        //    _eventModels.Enqueue(model);
        //    //Semaphore.Release();
        //}
    }
}

