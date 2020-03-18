using Castle.DynamicProxy;
using Tomato.Rpc.Client;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using Tomato.Rpc;
using Tomato.Rpc.Exceptions;
using Tomato.Rpc.Internal;
using Tomato.Rpc.Protocol;
using Tomato.Rpc.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Tomato.Extra
{
    public class DynamicClientProxy : IClientProxy
    {
        private readonly IProxyGenerator _generator;
        private IInterceptor _remoteInvoker;
        private IInterceptor _localInvoker;
        private readonly IServiceProvider _provider;

        private IServiceRouter _serviceRouter;
        private IServiceActorLocator<AmpMessage> _actorLocator;


        private static readonly ConcurrentDictionary<string,object> TYPE_CACHE
            = new ConcurrentDictionary<string, object>();

        public DynamicClientProxy(IServiceProvider provider)
        {
            this._generator = provider.GetRequiredService<IProxyGenerator>();

            this._provider = provider;
        }


        private IInterceptor RemoteCallInterceptor =>
            this._remoteInvoker ??
            (this._remoteInvoker = this._provider.GetRequiredService<RemoteInvokeInterceptor>());

        private IInterceptor LocalInvokeInterceptor =>
            this._localInvoker ??
            (this._localInvoker = this._provider.GetRequiredService<LocalInvokeInterceptor>());




        private IServiceRouter ServiceRouter =>
            this._serviceRouter ??
            (this._serviceRouter = this._provider.GetRequiredService<IServiceRouter>());


        private IServiceActorLocator<AmpMessage> ActorLocator =>
            this._actorLocator ??
            (this._actorLocator = this._provider.GetRequiredService<IServiceActorLocator<AmpMessage> >());

        public TService Create<TService>(ushort spacialMessageId = 0 ) where TService : class
        {
            var serviceType = typeof(TService);
            var cacheKey = $"{serviceType.FullName}${spacialMessageId}";

            if(TYPE_CACHE.TryGetValue(cacheKey,out var cacheService))
            {
                return (TService)cacheService;
            }

            var service = serviceType.GetCustomAttribute(typeof(RpcServiceAttribute), false);
            if (service == null)
            {
                throw new InvalidOperationException($"Miss RpcServiceAttribute at {serviceType}");
            }
            var sAttr = service as RpcServiceAttribute;

            var serviceIdentity = InternalHelper.FormatServiceIdentity(sAttr.GroupName,sAttr.ServiceId, spacialMessageId);
            // $"{sAttr.ServiceId}${spacialMessageId};{sAttr.GroupName}";
            var servicePath= InternalHelper.FormatServicePath(sAttr.ServiceId, spacialMessageId);

            var isLocal = IsLocalCall(serviceIdentity);

            TService proxy;
            if (isLocal)
            {
                var actor = ActorLocator.LocateServiceActor(servicePath);

                if (!(actor is TService realService))
                {
                    throw new InvalidOperationException($"{serviceType.FullName} has no implementation class,it should  be deployed on a remote server");
                }
                proxy = this._generator.CreateInterfaceProxyWithTarget(realService, LocalInvokeInterceptor);
                TYPE_CACHE.TryAdd(cacheKey,proxy);

            }
            else
            {
                proxy = this._generator.CreateInterfaceProxyWithoutTarget<TService>(RemoteCallInterceptor);
                TYPE_CACHE.TryAdd(cacheKey,proxy);
            }

            return proxy;

        }

        private bool IsLocalCall(string serviceIdentity)
        {
            var point = ServiceRouter.FindRouterPoint(serviceIdentity).GetAwaiter().GetResult();
            if (point == null)
            {
                return true;
            }
            return point.RoutePointType == RoutePointType.Local;
        }

    }
}
