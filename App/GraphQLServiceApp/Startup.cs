using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphQL.Server;
using GraphQL.Types;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using PeiuPlatform.DataAccessor;
namespace PeiuPlatform.App
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMySqlDataAccessor();
            services.AddRedisDataAccess(Configuration);
            
            services.AddCors();

            services.AddHttpContextAccessor();
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    //ValidIssuer = UserClaimTypes.Issuer,
                    ValidAudience = "https://www.peiu.co.kr",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Program.Secret))

                };
                //options.ClaimsIssuer = UserClaimTypes.Issuer;


            });

            services.AddGraphQLObjects();
            //services.AddHttpContextAccessor();
            services.AddGraphQL(_ =>
            {
                _.EnableMetrics = true;
#if DEBUG
                _.ExposeExceptions = true;
#else
                _.ExposeExceptions = false;
#endif
            }).AddUserContextBuilder(context => new GraphQLUserContext { UserClaim = context.User });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            var withOrigins = Configuration.GetSection("AllowedOrigins").Get<string[]>();
            app.UseCors(builder =>
            {
                builder
                    //.AllowAnyOrigin()
                    .WithOrigins(withOrigins)
                    .AllowAnyHeader()
                    .WithMethods("GET", "POST", "PUT", "DELETE")
                    .AllowCredentials();
            });
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseRouting();
            app.UseGraphQL<ISchema>();
            
            //#if DEBUG
            app.UseGraphQLPlayground();
//#endif
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
