using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Power21.Device.Services;
using NLog.Extensions.Logging;
using Power21.Device.Dao;
using Newtonsoft.Json;
using System.IO;
using MQTTnet.Client.Options;

namespace Power21.Device
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var modbusConfig = Configuration.GetSection("Modbus").Get<ModbusConfig>();

            
            string mappingFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mapping.json");
            Map map = JsonConvert.DeserializeObject<Map>(File.ReadAllText(mappingFile));
            services.AddSingleton(map);
            services.AddSingleton(modbusConfig);
            services.AddControllers();
            services.AddLogging(log =>
            {
                log.ClearProviders();
                log.SetMinimumLevel(LogLevel.Trace);
                log.AddNLog(Configuration);
            });
            
            services.AddSingleton<IMqttQueue, MqttQueue>();
            services.AddSingleton<IModbusInterface, ModbusInterface>();
            services.AddHostedService<MqttRecievingWorker>();
            services.AddHostedService<HeartBeatWorker>();
            services.AddHostedService<ModbusMonitorWorker>();
            //services.AddHostedService<WritingWorker>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
