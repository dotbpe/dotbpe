using Tomato.Rpc;
using Tomato.Rpc.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Tomato.Gateway
{
    public class HttpServiceScanner:IHttpServiceScanner
    {
        private readonly IClientProxy _proxy;
        private readonly IServiceProvider _provider;
        private readonly ILogger<HttpServiceScanner> _logger;

        private readonly MethodInfo _proxyCreate;

        private HttpRouteOptions _options;

        private readonly ushort specialMessageId = 0;
        public HttpServiceScanner(
            IClientProxy proxy,
            IServiceProvider provider,
            ILogger<HttpServiceScanner> logger)
        {
            this._proxy = proxy;

            _proxyCreate = proxy.GetType().GetMethod("Create");

            this._provider = provider;
            this._logger = logger;
        }
        public HttpRouteOptions Scan(string dllPrefix="*",params string[] categories)
        {
            this._logger.LogInformation(dllPrefix);

            _options = new HttpRouteOptions();

            string basePath = Rpc.Internal.Environment.GetAppBasePath();

            var dllFiles = Directory.GetFiles(string.Concat(basePath, ""), $"{dllPrefix}.dll");


            this._logger.LogInformation("dll count={0}",dllFiles.Length);

            List<Assembly> assemblies = new List<Assembly>();
            foreach (var file in dllFiles)
            {

                assemblies.Add(Assembly.LoadFrom(file));
            }

            this._logger.LogInformation("assembly count={0}", assemblies.Count);

            foreach (var a in assemblies)
            {
                //Console.WriteLine(a.FullName);
                foreach (var type in a.GetTypes())
                {

                    if (!type.IsInterface)
                       continue;

                    //this._logger.LogInformation(type.FullName);
                    var sAttr = type.GetCustomAttribute<RpcServiceAttribute>();
                    if (sAttr == null)
                        continue;

                    AddRpcService(type, sAttr, _options,categories);
                }
            }
            return _options;
        }

        public HttpRouteOptions GetRuntimeRouteOptions()
        {
            return _options;
        }
        private void AddRpcService(Type type, RpcServiceAttribute sAttr, HttpRouteOptions options,params string[] categories)
        {
            var methods = type.GetMethods();
            foreach (var m in methods)
            {
                var mAttr = m.GetCustomAttribute<RpcMethodAttribute>();
                if (mAttr == null)
                    continue;

                var rAttr = m.GetCustomAttribute<RouterAttribute>();
                if (rAttr == null)
                    continue;

                if (categories != null && categories.Any())
                {
                    if (categories.Contains(rAttr.Category))
                    {
                        AddHttpServiceRouter(type,m, sAttr, mAttr, rAttr, options);
                    }
                }
                else
                {
                    if( "default".Equals(rAttr.Category, StringComparison.OrdinalIgnoreCase))
                    {
                        AddHttpServiceRouter(type, m, sAttr, mAttr, rAttr, options);
                    }
                }
            }
        }

        private void AddHttpServiceRouter(Type type,MethodInfo m, RpcServiceAttribute sAttr, RpcMethodAttribute mAttr,
            RouterAttribute rAttr, HttpRouteOptions options)
        {
            var item = new RouteItem
            {
                Path = rAttr.Path,
                AcceptVerb = rAttr.AcceptVerb,
                Category = rAttr.Category,
                Version = rAttr.Version,
                InvokeMethod = m,
                MessageId = mAttr.MessageId,
                ServiceId = sAttr.ServiceId
            };

            //special MessageId;
            var args = new object[] {this.specialMessageId};

            item.InvokeService = this._proxyCreate.MakeGenericMethod(type).Invoke(this._proxy, args);

            if (rAttr.PluginType != null )
            {
                item.Plugin = ActivatorUtilities.CreateInstance(this._provider, rAttr.PluginType) as IHttpPlugin;
            }
            options.Items.Add(item);

            _logger.LogDebug("url:{0},verb:{1},service:{2},method:{3}",
                item.Path,item.AcceptVerb,type.Name.Split('.').Last(),m.Name);

        }
    }
}
