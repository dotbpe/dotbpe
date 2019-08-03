using System;
using Tomato.Rpc.Client.RouterPolicy;
using Tomato.Rpc.Config;
using Tomato.Rpc.Protocol;
using Tomato.Rpc.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Peach;
using Peach.Protocol;

namespace Tomato.Rpc.Client
{
    public class ClientProxyFactory:IClientProxyFactory
    {
        private readonly IServiceCollection _container;
        private IServiceProvider _provider;
        private ClientProxyFactory(IServiceCollection container)
        {
            this._container = container;
        }


        /// <summary>
        /// create client proxy factor
        /// </summary>
        /// <param name="container">if container is null container will be create new instance inside</param>
        /// <returns></returns>
        public static IClientProxyFactory Create(IServiceCollection container =null)
        {
            if (container == null)
            {
                container = new ServiceCollection();
            }

            container.AddSingleton<IChannelHandlerPipeline, AmpChannelHandlerPipeline>();
            container.AddSingleton<IServiceRouter, DefaultServiceRouter>();
            container.AddSingleton<ISocketClient<AmpMessage>, RpcSocketClient>();
            container.AddSingleton<IServiceActorLocator<AmpMessage>, DefaultServiceActorLocator>();
            container.AddSingleton<ICallInvoker, DefaultCallInvoker>();
            container.AddSingleton<IClientMessageHandler<AmpMessage>, DefaultClientMessageHandler>();
            container.AddSingleton<IRpcClient<AmpMessage>, DefaultRpcClient>();
            container.AddSingleton<ITransportFactory<AmpMessage>, DefaultTransportFactory>();
            container.TryAddSingleton<IClientAuditLoggerFactory,DefaultClientAuditLoggerFactory>();
            container.TryAddSingleton<IRequestAuditLoggerFactory,DefaultRequestAuditLoggerFactory>();
            container.TryAddSingleton<IRouterPolicy, RoundrobinPolicy>();
            container.Configure<RpcClientOptions>(x => { });
            container.AddLogging();
            container.AddOptions();

            return new ClientProxyFactory(container);
        }


        /// <summary>
        /// configure some options
        /// </summary>
        /// <param name="configureOptions"></param>
        /// <typeparam name="TOption"></typeparam>
        /// <returns></returns>
        public IClientProxyFactory Configure<TOption>(Action<TOption> configureOptions) where TOption : class
        {
            this._container.Configure(configureOptions);
            return this;
        }

        public TService GetService<TService>() where TService : class
        {
            if (this._provider == null)
            {
                this._provider = this._container.BuildServiceProvider();
            }

            return this._provider.GetService<TService>();
        }

        /// <summary>
        /// add other dependency services
        /// </summary>
        /// <param name="configServicesDelegate"></param>
        /// <returns></returns>
        public IClientProxyFactory AddDependencyServices(Action<IServiceCollection> configServicesDelegate)
        {
            configServicesDelegate(this._container);
            return this;
        }

        /// <summary>
        /// get client proxy  instance
        /// </summary>
        /// <returns></returns>
        public IClientProxy GetClientProxy()
        {
            return GetService<IClientProxy>();
        }
    }
}
