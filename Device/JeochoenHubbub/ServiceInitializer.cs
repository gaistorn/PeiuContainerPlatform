using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet.Client.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace PeiuPlatform.Hubbub
{
    public static class ServiceInitializer
    {
        public static void Configuration(string[] args, HostBuilderContext hostContext, IServiceCollection services)
        {
            try
            {
                LoadModels(services);
                LoadConfiguration(hostContext, services);
                RegistrationServices(services);
            }
            catch(Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex, ex.Message);
            }
        }

        #region #Logic#
        private static void LoadModels(IServiceCollection services)
        {
            Model.TemplateRoot root = new Model.TemplateRoot();
            string mappingFileFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PushTemplate");
            root.PushModels = JsonFileReader.OpenFileInFolder<Model.PushModel>(mappingFileFolder);
            services.AddSingleton(root);
        }

        private static void LoadConfiguration(HostBuilderContext hostContext, IServiceCollection services)
        {
            HubbubInformation hubbubConfig = hostContext.Configuration.GetSection("hubbub").Get<HubbubInformation>();
            MqttClientTcpOptions mqttClientTcpOptions = hostContext.Configuration.GetSection("Mqtt").Get<MqttClientTcpOptions>();
            services.AddSingleton(mqttClientTcpOptions);
            services.AddSingleton(hubbubConfig);
        }

        private static void RegistrationServices(IServiceCollection services)
        {
            services.AddSingleton<IGlobalStorage, GlobalStorage>();
            services.AddSingleton<IZKFactory, ZKFactory>();
            services.AddHostedService<MqttReadWriteWorker>();
            services.AddHostedService<ModbusReadWriteWorker>();
        }
        #endregion
    }
}
