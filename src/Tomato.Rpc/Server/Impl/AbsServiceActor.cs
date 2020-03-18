using Tomato.Rpc.Protocol;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Peach;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Environment = Tomato.Rpc.Internal.Environment;
using Tomato.Rpc.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Tomato.Rpc.Server
{
    public abstract class AbsServiceActor : IServiceActor<AmpMessage>
    {
        private ILogger _logger;
        protected ILogger Logger
        {
            get
            {
                if (this._logger != null) return this._logger;

                var loggerFactory = Environment.ServiceProvider.GetRequiredService<ILoggerFactory>();
                this._logger = loggerFactory.CreateLogger(GetType());
                return this._logger;
            }
            set => this._logger = value;
        }

        public abstract string Id { get; }

        public abstract string GroupName { get; }

        /// <summary>
        /// process receive message from remote client
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task ReceiveAsync(ISocketContext<AmpMessage> context, AmpMessage message)
        {

            AmpMessage rsp =null;
            try
            {
                TomatoDiagnosticListenerExtensions.Listener.ServiceActorReceiveRequest(context, message);

                Logger.LogDebug("Receive message,Id={0}", message.Id);
                rsp = await ProcessAsync(context, message);
                rsp.Sequence = message.Sequence; //通讯请求序列

                await context.SendAsync(rsp);

            }
            catch (ClosedChannelException closedEx)
            {
                Logger.LogError(closedEx, "Receive message occ error,channel closed,{messageId}", message.Id);
                TomatoDiagnosticListenerExtensions.Listener.ServiceActorException(context, message, closedEx);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Receive message occ error ,messageId={messageId}", message.Id);
                rsp = await SendErrorResponseAsync(context, message).ConfigureAwait(false);
                TomatoDiagnosticListenerExtensions.Listener.ServiceActorException(context, message, ex);
            }
            finally
            {
                TomatoDiagnosticListenerExtensions.Listener.ServiceActorSendResponse(context,message,rsp);
            }
        }

        /// <summary>
        /// remote call
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        protected abstract Task<AmpMessage> ProcessAsync(ISocketContext<AmpMessage> context, AmpMessage req);


        /// <summary>
        /// 发送服务端意外错误的消息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMessage"></param>
        /// <returns></returns>
        private async Task<AmpMessage> SendErrorResponseAsync(ISocketContext<AmpMessage> context, AmpMessage reqMessage)
        {
            var rsp = AmpMessage.CreateResponseMessage(reqMessage.ServiceId, reqMessage.MessageId);
            rsp.InvokeMessageType = InvokeMessageType.Response;
            rsp.Sequence = reqMessage.Sequence;
            rsp.Code = RpcErrorCodes.CODE_INTERNAL_ERROR; //内部错误
            try
            {
                await context.SendAsync(rsp).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "send error response fail:" + ex.Message);
            }

            return rsp;
        }
    }
}
