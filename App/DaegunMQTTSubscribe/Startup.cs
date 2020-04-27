using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PeiuPlatform.Lib;

namespace PeiuPlatform.App
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILoggerFactory logger)
        {
            Configuration = configuration;
            loggerFactory = logger;
        }

//        public Startup(IConfiguration configuration) { }

        public IConfiguration Configuration { get; }
        public ILoggerFactory loggerFactory { get; }
   

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IPacketQueue, PacketQueue>();

            EventPublisherWorker eventPublisher = new EventPublisherWorker(2);
            eventPublisher.Initialize();
            services.AddSingleton(eventPublisher);
            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, ConsumeDataService>();
            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, MqttHostService>();
            //IBackgroundMongoTaskQueue queue_service = new MongoBackgroundTaskQueue();
            //services.AddSingleton(queue_service);

            //services.AddHostedService<MongoBackgroundHostService>();
            //MQTTDaegunSubscribe describe = new MQTTDaegunSubscribe(loggerFactory, queue_service, mqttOptions);
            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseCors(builder =>
            //{
            //    builder
            //        .WithOrigins("http://www.peiu.co.kr")
            //        .WithOrigins("http://www.peiu.co.kr:30000")
            //        .WithOrigins("http://www.peiu.co.kr:30001")
            //        .WithOrigins("http://192.168.0.88:3535")
            //        .WithOrigins("http://192.168.0.25:3535")
            //        .WithOrigins("http://192.168.0.25:30000")
            //        //.WithOrigins("http://localhost")
            //        //.WithOrigins("http://localhost:3333")
            //        //.WithOrigins("http://127.0.0.1:3333")
            //        //.WithOrigins("http://127.0.0.1:3333")
            //        //.WithOrigins("http://192.168.0.17:3333")
            //        //.WithOrigins("http://210.96.71.134:3333")
            //        .AllowAnyHeader()
            //        .WithMethods("GET", "POST", "PUT", "DELETE")
            //        .AllowCredentials();
            //});

            //app.UseMvc();
        }
    }
}
