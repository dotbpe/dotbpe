
using System;
using System.IO;
using System.Reflection;
using DotBPE.Extra;
using DotBPE.Gateway;
using DotBPE.Gateway.Swagger;
using DotBPE.Gateway.Swagger.Generator;
using DotBPE.Gateway.SwaggerUI;
using DotBPE.Rpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace GreeterHttpService
{
    public class Startup
    {
        public Startup(IConfiguration config)
        {
            this.Configuration = config;
        }

        IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //添加路由信息
            services.BindService<GreeterService>(); //bindService
            services.BindService<SwaggerSampleService>();

            services.AddGateway("GreeterHttpService"); //add gateway and auto scan router infos

            services.AddMessagePackSerializer(); //message pack serializer
            services.AddJsonNetParser(); // http result json parser
            services.AddDynamicClientProxy(); // aop client
            services.AddDynamicServiceProxy(); // aop service

            services.AddSwagger();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseSwagger( config =>
            {
                //config.RoutePath = "/v2/swagger.json";
                config.ApiInfo = new SwaggerApiInfo
                {
                    Title = "GreetingService",
                    Description = "测试Swagger",
                    Version = "1.0"
                };

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                config.IncludeXmlComments(xmlPath);
            });

            //app.UseStaticFiles();
            app.UseSwaggerUI(config => { config.RoutePrefix = "/swagger"; });
            //use gateway middleware
            app.UseGateway();
        }
    }
}
