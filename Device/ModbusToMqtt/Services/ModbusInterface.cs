using NModbus;
using Power21.Device.Dao;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Power21.Device.Services
{
    public interface IModbusInterface
    {
        void SetValue(string Key, IComparable Value);

        Task<IComparable> GetValue(string key, CancellationToken cancellationToken);
        
    }

    public class ModbusInterface : IModbusInterface
    {
        private ConcurrentDictionary<string, IComparable> _valueMaps =
            new ConcurrentDictionary<string, IComparable>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public ModbusInterface()
        {
            //var factory = new ModbusFactory();
            //modbusMaster = factory.CreateMaster()
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
    }
}

