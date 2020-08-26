using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform.Hubbub
{
    public interface IAsyncDataAccessor
    {
        void SetValue(string Key, IComparable Value);
        Task<IComparable> GetValue(string key, CancellationToken cancellationToken);
        Task<IComparable> GetValue(string key);
        bool ContainKey(string Key);
    }
    public interface IGlobalStorage<TEventModel> : IAsyncDataAccessor
    {
        Task<JObject> BindingAndCopy(JObject obj, CancellationToken cancellationToken);

        Task<IEnumerable< ModbusWriteCommand>> GetWriteValues(CancellationToken cancellationToken);
        void SetWriteValues(ModbusWriteCommand command);

        Task<IEnumerable<TEventModel>> GetEventModels(CancellationToken cancellationToken);
        void SetEventValues(TEventModel model);
    }

    public class GlobalStorage<TEventModel> : IGlobalStorage<TEventModel>
    {
        private ConcurrentDictionary<string, IComparable> _valueMaps =
            new ConcurrentDictionary<string, IComparable>();

        private ConcurrentQueue<ModbusWriteCommand> _writeQueue = new ConcurrentQueue<ModbusWriteCommand>();

        private ConcurrentQueue<TEventModel> _eventModels = new ConcurrentQueue<TEventModel>();

        protected SemaphoreSlim Semaphore = new SemaphoreSlim(0);

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

        public bool ContainKey(string Key)
        {
            return _valueMaps.ContainsKey(Key);
        }

        public virtual async Task<IComparable> GetValue(string key)
        {
            await Semaphore.WaitAsync();
            return _valueMaps.GetValueOrDefault(key);
        }

        public virtual async Task<IComparable> GetValue(string key, CancellationToken cancellationToken)
        {
            await Semaphore.WaitAsync(cancellationToken);
            return _valueMaps.GetValueOrDefault(key);
        }

        public virtual void SetValue(string Key, IComparable Value)
        {
            _valueMaps[Key] = Value;
            Semaphore.Release();
        }

        private async Task ValidateAndBinding(JToken field, CancellationToken cancellationToken)
        {
            string fieldName = field.Value<string>();
            if (fieldName.StartsWith('$'))
            {
                string variable_name = fieldName.TrimStart('$');
                if(variable_name == "PCS#3.ActivePower")
                {

                }
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

        public async Task<IEnumerable<ModbusWriteCommand>> GetWriteValues(CancellationToken cancellationToken)
        {
            await Semaphore.WaitAsync(cancellationToken);
            List<ModbusWriteCommand> commands = new List<ModbusWriteCommand>();
            while (_writeQueue.TryDequeue(out ModbusWriteCommand modbusWriteCommand))
                commands.Add(modbusWriteCommand);
            return commands;
        }

        public void SetWriteValues(ModbusWriteCommand command)
        {
            _writeQueue.Enqueue(command);
            Semaphore.Release();
        }

        public async Task<IEnumerable<TEventModel>> GetEventModels(CancellationToken cancellationToken)
        {
            //await Semaphore.WaitAsync(cancellationToken);
            //List<EventModel> evtmodels = new List<EventModel>();
            //while (_eventModels.TryDequeue(out EventModel model))
            //    evtmodels.Add(model);
            //return evtmodels;
            return _eventModels;
        }

        public void SetEventValues(TEventModel model)
        {
            _eventModels.Enqueue(model);
            //Semaphore.Release();
        }
    }
}

