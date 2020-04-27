using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Criterion;
using PeiuPlatform.DataAccessor;
namespace PeiuPlatform.App
{
    public class GreeterService : Proto.HubbubService.HubbubServiceBase
    {
        private readonly ILogger<GreeterService> _logger;
        private readonly MysqlDataAccessor mysqlDataAccessor;
        public GreeterService(ILogger<GreeterService> logger, MysqlDataAccessor mysqlDataAccessor)
        {
            _logger = logger;
            this.mysqlDataAccessor = mysqlDataAccessor;
        }

        private async Task<IList<T>> SelectQuery<T>(IStatelessSession session, ICriterion criterion) where T : class
        {
            return await session.CreateCriteria<T>().Add(criterion).ListAsync<T>();
        }

        public override async Task<Proto.ReplyModbusDataPoints> GetAllModbusDataPoints(Proto.RequestProtocolId request, ServerCallContext context)
        {
            using (var session = mysqlDataAccessor.SessionFactory.OpenStatelessSession())
            {
                Proto.ReplyModbusDataPoints reply = new Proto.ReplyModbusDataPoints();
                
                var analogPoints = await SelectQuery<ModbusAnalogPoint>(session, Restrictions.Eq("Protocolid", request.Protocolid));
                var digitalinputpoints = await SelectQuery<ModbusDigitalInputPoint>(session, Restrictions.Eq("Protocolid", request.Protocolid));
                var digitaloutputpoints = await SelectQuery<ModbusDigitalOutputPoint>(session, Restrictions.Eq("Protocolid", request.Protocolid));

                var proto_analogpoints = ConvertList<Proto.ModbusAnalogDataPoint, ModbusAnalogPoint>(analogPoints);
                var proto_digitalinputpoints = ConvertList<Proto.ModbusDigitalInputPoint, ModbusDigitalInputPoint>(digitalinputpoints);
                var proto_digitaloutputpoints = ConvertList<Proto.ModbusDigitalOutputPoint, ModbusDigitalOutputPoint>(digitaloutputpoints);

                reply.Analogpoints.AddRange(proto_analogpoints);
                reply.Digitalinputpoints.AddRange(proto_digitalinputpoints);
                reply.Digitaloutputpoints.AddRange(proto_digitaloutputpoints);

                return reply;
            }
        }

        private IEnumerable<TResult> ConvertList<TResult,TInput>(IEnumerable<TInput> inputs) where TResult : class, new() where TInput : class
        {
            foreach(TInput input in inputs)
            {
                TResult result = null;
                if (TryConvert<TInput, TResult>(input, ref result))
                {
                    yield return result;
                }
            }
        }

        private bool TryConvert<T,S>(T input, ref S output) where T : class where S : class, new()
        {
            try
            {
                var input_props = typeof(T).GetProperties();
                var output_props = typeof(S).GetProperties();

                output = new S();
                foreach (var property in output_props)
                {
                    if (property.CanWrite == false)
                        continue;
                    var src_property = input_props.FirstOrDefault(x => x.Name.ToUpper() == property.Name.ToUpper() && x.CanRead);
                    if (src_property == null)
                        continue;
                    object output_value = src_property.GetValue(input);
                    if (output_value == null && property.PropertyType == typeof(string))
                        output_value = "";
                    if (src_property.PropertyType != property.PropertyType)
                    {
                        if(property.PropertyType.IsEnum)
                        {
                            output_value = Enum.ToObject(property.PropertyType, output_value);
                        }
                        else
                            output_value = Convert.ChangeType(output_value, property.PropertyType);
                    }
                    property.SetValue(output, output_value);
                }
                return true;
            }
            catch(Exception ex)
            {
                output = null;
                return false;
            }

        }

        public override async Task<Proto.ModbusHubbubReply> GetAllModbusHubbubsBySiteId(Proto.RequestSiteid request, ServerCallContext context)
        {
            using(var session = mysqlDataAccessor.SessionFactory.OpenStatelessSession())
            {
                var result = await session.CreateCriteria<ModbusHubbub>().Add(Restrictions.Eq("Siteid", request.Siteid)).ListAsync<ModbusHubbub>();

                Proto.ModbusHubbubReply reply = new Proto.ModbusHubbubReply();
                foreach(ModbusHubbub hubbub in result)
                {
                    var row = new Proto.ModbusHubbubProto
                    {
                        Siteid = hubbub.Siteid,
                        Deviceindex = hubbub.Deviceindex,
                        Protocolid = hubbub.Protocolid,
                        Label = hubbub.Label,
                       // Description = hubbub.Description,
                        Host = hubbub.Host,
                        Port = hubbub.Port,
                        Slaveid = hubbub.Slaveid,
                        Scheme = hubbub.Scheme,
                        Timeoutms = hubbub.Timeoutms,

                    };

                    var analogPoints = await SelectQuery<ModbusAnalogPoint>(session, Restrictions.Eq("Protocolid", hubbub.Protocolid));
                    var digitalinputpoints = await SelectQuery<ModbusDigitalInputPoint>(session, Restrictions.Eq("Protocolid", hubbub.Protocolid));
                    var digitaloutputpoints = await SelectQuery<ModbusDigitalOutputPoint>(session, Restrictions.Eq("Protocolid", hubbub.Protocolid));

                    var proto_analogpoints = ConvertList<Proto.ModbusAnalogDataPoint, ModbusAnalogPoint>(analogPoints);
                    var proto_digitalinputpoints = ConvertList<Proto.ModbusDigitalInputPoint, ModbusDigitalInputPoint>(digitalinputpoints);
                    var proto_digitaloutputpoints = ConvertList<Proto.ModbusDigitalOutputPoint, ModbusDigitalOutputPoint>(digitaloutputpoints);
                    row.Analogpoints.AddRange(proto_analogpoints);
                    row.Digitalinputpoints.AddRange(proto_digitalinputpoints);
                    row.Digitaloutputpoints.AddRange(proto_digitaloutputpoints);


                    reply.Modbushubbubs.Add(row);
                }
                return reply;
            }
        }

        //override 
    }
}
