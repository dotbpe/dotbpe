using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;


namespace GreeterHttpService
{
    static class Program
    {
        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(100, 100);
            BuildWebHost(args).Run();
        }

        static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:5560") //HTTP绑定在6200端口
                .UseStartup<Startup>()
                .ConfigureLogging(builder => { builder.SetMinimumLevel(LogLevel.Warning); })
                .Build();
    }
}
