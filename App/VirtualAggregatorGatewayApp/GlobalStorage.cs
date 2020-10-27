using PeiuPlatform.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform
{
    public interface IGlobalStorage
    {
        ConcurrentQueue<ModbusControlModel> ControlModelQueues { get; }
        ConcurrentQueue<DataMessage> DataMessageQueues { get; }
    }
    public class GlobalStorage : IGlobalStorage
    {
        private ConcurrentQueue<ModbusControlModel> modbusControlModels = new ConcurrentQueue<ModbusControlModel>();
        public ConcurrentQueue<ModbusControlModel> ControlModelQueues => modbusControlModels;

        private ConcurrentQueue<DataMessage> dataMessageQueues = new ConcurrentQueue<DataMessage>();
        public ConcurrentQueue<DataMessage> DataMessageQueues => dataMessageQueues;

        public GlobalStorage()
        {

        }
    }
}
