using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog.Extensions.Logging;
using PeiuPlatform.Model.ExchangeModel;
using RestSharp;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace Hubbub
{
    public static class ServiceInitializer
    {
        private const string HubbubTemplateFile = "hubbubtemplate{0}.json";
        private const string HubbubTemplateTempFile = "hubbubtemplate.json.tmp";

        public static void Configuration(string[] args, IConfiguration configuration, IServiceCollection services)
        {
            try
            {
                BuildLogger(services, configuration);
                LoadModels(services);
                LoadConfiguration(configuration, services);
                RegistrationServices(services);
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex, ex.Message);
            }
        }

        private static void DownloadAndRegistHubbubInformation(int hubbubid, IConfiguration configuration, IServiceCollection services)
        {
            string RestApiServerAddress = configuration.GetSection("RestServerAddress").Value;
            string AccessToken = configuration.GetSection("AccessToken").Value;

            Task<ModbusHubbubMappingTemplate> t = DownloadAnalogTemplateAsync(hubbubid, RestApiServerAddress, AccessToken);
            t.Wait();
            services.AddSingleton(t.Result);
        }

        private static async Task<ModbusHubbubMappingTemplate> DownloadAnalogTemplateAsync(int hubbubid, string RestApiServerAddress, string AccessToken)
        {
            ModbusHubbubMappingTemplate AnalogTemplates = null;
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("Hubbub 템플릿 다운로드를 시도합니다");
            string templateOutputFile = string.Format(HubbubTemplateFile, hubbubid);
            try
            {

                var hubbubClient = new RestClient(RestApiServerAddress);
                hubbubClient.Authenticator = new RestSharp.Authenticators.JwtAuthenticator(AccessToken);
                var request = new RestRequest($"/api/Hubbub/v1/information/{hubbubid}");
                request.Parameters.Add(new Parameter("hubbubid ", hubbubid, ParameterType.GetOrPost));
                request.Parameters.Add(new Parameter("compress", true, ParameterType.GetOrPost));
                var str_result = await hubbubClient.GetAsync<string>(request);

                byte[] datas = Convert.FromBase64String(str_result);

                using (var inputStream = new MemoryStream(datas))
                using (var gZipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                using (var outputStream = new MemoryStream())
                {
                    gZipStream.CopyTo(outputStream);
                    var outputBytes = outputStream.ToArray();

                    string decompressed = System.Text.Encoding.UTF8.GetString(outputBytes);
                    File.WriteAllText(HubbubTemplateTempFile, decompressed, System.Text.Encoding.UTF8);
                    File.Move(HubbubTemplateTempFile, templateOutputFile, true);
                    AnalogTemplates = JsonConvert.DeserializeObject<ModbusHubbubMappingTemplate>(decompressed);
                    logger.Info("정상적으로 Hubbub 템플릿을 다운로드했습니다");
                }
            }
            catch (Exception ex)
            {
                if (File.Exists(HubbubTemplateFile))
                {
                    AnalogTemplates = JsonConvert.DeserializeObject<ModbusHubbubMappingTemplate>(File.ReadAllText(templateOutputFile, System.Text.Encoding.UTF8));
                    logger.Warn(ex, $"템플릿 정보를 가져오는 것에 실패했습니다. 기존의 템플릿 정보를 반영합니다");
                }
                else
                {
                    logger.Error(ex, $"템플릿 정보를 가져오는 것에 실패했습니다. 다음 {RestApiServerAddress} 서버의 접속 오류\n{ex.Message}");
                    throw;
                    //throw new Exception($"템플릿 정보를 가져오는 것에 실패했습니다. 다음 {RestApiServerAddress} 서버의 접속 오류");
                }
            }
            return AnalogTemplates;
        }

        #region #Logic#

        private static void LoadModels(IServiceCollection services)
        {
            //Model.TemplateRoot root = new Model.TemplateRoot();
            //string mappingFileFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PushTemplate");
            //root.PushModels = JsonFileReader.OpenFileInFolder<Model.PushModel>(mappingFileFolder);
            //services.AddSingleton(root);
        }

        private static void LoadConfiguration(IConfiguration configuration, IServiceCollection services)
        {
            HubbubInformation hubbubConfig = configuration.GetSection("hubbub").Get<HubbubInformation>();
            HubbubInformation.GlobalHubbubInformation = hubbubConfig;
            MqttClientTcpOptions mqttClientTcpOptions = configuration.GetSection("Mqtt").Get<MqttClientTcpOptions>();

            DownloadAndRegistHubbubInformation(hubbubConfig.hubbubid, configuration, services);

#if USE_REDIS
            var redisConfiguration = configuration.GetSection("redis").Get<RedisConfiguration>();
            ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfiguration.ConfigurationOptions);
            services.AddSingleton(connectionMultiplexer);
#else
            //services.AddSingleton<IAsyncDataAccessor<JObject>, GlobalStorage<JObject>>();
            services.AddSingleton<ConcurrentQueue<PushModel>>();
            services.AddSingleton<ConcurrentQueue<EventModel>>();
            services.AddSingleton<ConcurrentQueue<ModbusControlModel>>();
#endif
            services.AddSingleton(mqttClientTcpOptions);
            //services.AddSingleton(hubbubConfig);

            
        }

        private static void BuildLogger(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(logger =>
            {
                logger.ClearProviders();
#if DEBUG
                logger.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
#else
                logger.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
#endif
                logger.AddNLog(configuration);
            });
        }

       

        private static void RegistrationServices(IServiceCollection services)
        {
            
            services.AddHostedService<ModbusAcquisitionService>();
            services.AddHostedService<MqttReadWriteWorker>();
        }
#endregion
    }
}
