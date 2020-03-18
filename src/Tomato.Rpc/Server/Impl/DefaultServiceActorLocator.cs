using System;
using System.Collections.Concurrent;
using System.Linq;
using Tomato.Rpc.Exceptions;
using Tomato.Rpc.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Tomato.Rpc.Server
{
    public class DefaultServiceActorLocator : IServiceActorLocator<AmpMessage>
    {
        private static readonly HeartbeatActor HeartbeatActor = new HeartbeatActor();
        private readonly ILogger<DefaultServiceActorLocator> _logger;

        private readonly ConcurrentDictionary<string, IServiceActor<AmpMessage>> ACTOR_CACHE =
            new ConcurrentDictionary<string, IServiceActor<AmpMessage>>();

        public DefaultServiceActorLocator(IServiceProvider serviceProvider)
        {
            var logFactory = serviceProvider.GetService<ILoggerFactory>();
            this._logger = logFactory.CreateLogger<DefaultServiceActorLocator>();
            Initialize(serviceProvider);
        }

        /// <summary>
        ///     Service Actor Locate
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public IServiceActor<AmpMessage> LocateServiceActor(string servicePath)
        {
            if (HeartbeatActor.Id.Equals(servicePath)) return HeartbeatActor;

            var path = servicePath.Split('.');
            string serviceId;
            string messageId;
            if (path.Length == 2)
            {
                serviceId = path[0];
                messageId = path[1];
            }
            else if (path.Length == 3)
            {
                serviceId = path[1];
                messageId = path[2];
            }
            else
            {
                this._logger.LogError("service path error:{servicePath}", servicePath);
                throw new RpcException($"service path error:{servicePath}");
            }


            var serviceKey = $"{serviceId}.0";
            var messageKey = $"{serviceId}.{messageId}";

            var actor = GetFromCache(messageKey);
            if (actor != null) return actor;


            actor = GetFromCache(serviceKey);

            if (actor != null) return actor;

            return GetNotFoundActor();
        }

        private void Initialize(IServiceProvider serviceProvider)
        {
            var actorList = serviceProvider.GetServices<IServiceActor<AmpMessage>>();
            if (actorList != null && actorList.Any())
                foreach (var actor in actorList)
                    this.ACTOR_CACHE.AddOrUpdate(actor.Id, actor, (k, v) => actor);
            else
                this._logger.LogWarning("no service actor was registered");
        }

        protected virtual IServiceActor<AmpMessage> GetFromCache(string cacheKey)
        {
            return
                this.ACTOR_CACHE.TryGetValue(cacheKey, out var serviceActor)
                    ? serviceActor
                    : null;
        }

        protected virtual IServiceActor<AmpMessage> GetNotFoundActor()
        {
            return NotFoundServiceActor.Default;
        }
    }
}
