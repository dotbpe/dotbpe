using Consul;
using Tomato.Baseline.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tomato.Rpc;
using Tomato.Extra;
using Tomato.Rpc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Tomato.Rpc.Server;
using Tomato.Rpc.Client;

namespace MathService
{
    static class Program
    {
        static void Main(string[] args)
        {
            /* */
            var builder = new HostBuilder()
             .UseRpcServer()
             .UseCastleDynamicProxy()
             .UseMessagePackSerializer()
                          /*.BindService<Definition.MathService>()

              * 这段用于测试审计日志*/
              .BindServices(services => { services.Add<Definition.ExtraCallFooMathService>().Add<Definition.FooService>();})
             .ConfigureServices(s =>
             {
                 s.AddSingleton<IAuditLoggerFormat, AuditLoggerFormat>();
                 s.AddSingleton<IRequestAuditLoggerFactory, RequestAuditLoggerFactory>();
                 s.AddSingleton<IClientAuditLoggerFactory, ClientAuditLoggerFactory>();
             })
             .ConfigureLogging(
                 logger =>
                 {
                     logger.AddConsole();
                 }
             );

            //启动
            builder.RunConsoleAsync().Wait();


            /**
             * Consul Service Registration
             *
             var builder = new HostBuilder()
             .UseRpcServer()
             .UseCastleDynamicProxy()
             .UseMessagePack()
             .BindService<Definition.MathService>()
             //.BindServices(services => { services.Add<Definition.MathService>();})
             .UseConsulServiceRegistration((point, list) =>
             {
                 var tcpCheck = new AgentServiceCheck
                 {
                     DeregisterCriticalServiceAfter = 1.Minutes(),
                     Interval = 30.Seconds(),
                     TCP = EndPointParser.ParseEndPointToString(point.RemoteAddress)
                 };
                 list.Add(tcpCheck);
             }) //添加服务注册的依赖
             .ConfigureLogging(
                 logger =>
                 {
                     logger.AddConsole();
                 }
             );

            //启动
            builder.RegisterAndRunConsoleAsync().Wait();
            */


        }
    }
}
