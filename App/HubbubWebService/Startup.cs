using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PeiuPlatform.DataAccessor;
using PeiuPlatform.Model;
using PeiuPlatform.Model.Database;
using PeiuPlatform.Model.IdentityModel;

namespace PeiuPlatform.App
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
            services.AddMySqlDataAccessor();
            services.AddSingleton<IMqttPusher, MqttPusher>();
            services.AddControllers();
            ConfigureAuthrozation(services);
            services.AddCors();


            services.AddSwaggerDocumentation();
            services.AddHttpContextAccessor();
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Account2/AccessDenied2");
                options.Cookie.Domain = null;
                options.Cookie.Name = "PEIU.Auth.Cookie";
                //options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.HttpOnly = true;
                //options.ExpireTimeSpan = TimeSpan.FromMinutes(60);

                //options.LoginPath = new PathString("/api/auth/logintoredirec");
                //options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;


            });

            //services.AddIdentity<UserAccountEF, Role>(options =>
            //options.ClaimsIdentity.UserIdClaimType = "Id")
            //    .AddDefaultTokenProviders();

            //add the following line of code
            //services.AddScoped<IUserClaimsPrincipalFactory<UserAccountEF>, ClaimsPrincipalFactory>();

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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

            app.UseRouting();

            app.UseAuthorization();

            app.UseAuthentication();

            app.UseSwaggerDocumentation();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void ConfigureAuthrozation(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(UserPolicyTypes.AllUserPolicy,
                     policy => policy.RequireRole(UserRoleTypes.Aggregator, UserRoleTypes.Contractor, UserRoleTypes.Supervisor));
                options.AddPolicy(UserPolicyTypes.RequiredManager,
                    policy => policy.RequireRole(UserRoleTypes.Aggregator, UserRoleTypes.Supervisor));
                options.AddPolicy(UserPolicyTypes.OnlySupervisor,
                    policy => policy.RequireRole(UserRoleTypes.Supervisor));
            });

            // register the scope authorization handler
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
        }
    }
}
