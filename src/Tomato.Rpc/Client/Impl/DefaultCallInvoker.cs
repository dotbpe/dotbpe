using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Tomato.Rpc.Codec;
using Tomato.Rpc.Diagnostics;
using Tomato.Rpc.Exceptions;
using Tomato.Rpc.Protocol;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;

namespace Tomato.Rpc.Client
{
    public class DefaultCallInvoker : ICallInvoker
    {
        private readonly IClientMessageHandler<AmpMessage> _handler;
        private readonly IRpcClient<AmpMessage> _rpcClient;
        private readonly ILogger<DefaultCallInvoker> _logger;
        private readonly ISerializer _serializer;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<AmpMessage>> _resultDictionary = new ConcurrentDictionary<string, TaskCompletionSource<AmpMessage>>();

        private static int INVOKER_SEQ;
        //private static object lockObj = new object();

        public DefaultCallInvoker(
            IClientMessageHandler<AmpMessage> handler,
            IRpcClient<AmpMessage> rpcClient,
            ISerializer serializer,
            ILogger<DefaultCallInvoker> logger
        )
        {
            this._handler = handler;
            this._rpcClient = rpcClient;
            this._serializer = serializer;

            this._logger = logger;
            this._handler.OnReceived += _handler_OnReceived;
        }

        /// <summary>
        /// server response
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void _handler_OnReceived(object sender, AmpMessage message)
        {
            if (message.InvokeMessageType == InvokeMessageType.Response)
            {
                //处理消息
                MessageReceived(message);
            }
        }

        /// <summary>
        /// Async Call
        /// </summary>
        /// <param name="request"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private async Task<AmpMessage> AsyncCallInner(AmpMessage request, int timeout = 3000)
        {
            TomatoDiagnosticListenerExtensions.Listener.ClientSendRequest(request);
            AutoSetSequence(request);
            this._logger.LogDebug("new request id={0},type={1}", request.Id, request.InvokeMessageType);

            if (request.InvokeMessageType == InvokeMessageType.InvokeWithoutResponse)
            {
                await SendAsync(request);
                var rsp = AmpMessage.CreateResponseMessage(request.ServiceId, request.MessageId);
                TomatoDiagnosticListenerExtensions.Listener.ClientReceiveResponse(request, rsp);
                return rsp;
            }

            var cts = new CancellationTokenSource(timeout);
            //timeout callback
            using(cts.Token.Register(() => TimeOutCallBack(request.Id), false))
            {
                //register callback
                var callbackTask = RegisterResultCallbackAsync(request.Id);

                //async call
                await SendAsync(request);

                //get return message
                var rsp = await callbackTask;

                TomatoDiagnosticListenerExtensions.Listener.ClientReceiveResponse(request, rsp);
                return rsp;
            }
        }

        public async Task<RpcResult> AsyncNotify<T>(string callName, string groupName, int serviceId, ushort messageId, T req)
        {
            RpcResult result = new RpcResult();

            var reqMessage = AmpMessage.CreateRequestMessage(serviceId, messageId, true);
            reqMessage.FriendlyServiceName = callName;
            reqMessage.ServiceGroupName = groupName;
            reqMessage.CodecType = (CodecType) Enum.ToObject(typeof(CodecType), this._serializer.CodecType);
            reqMessage.Data = this._serializer.Serialize(req);
            var rsp = await AsyncCallInner(reqMessage);
            if (rsp != null)
            {
                result.Code = rsp.Code;
            }
            else
            {
                this._logger.LogError("Call {0} , return null", callName);
                result.Code = RpcErrorCodes.CODE_INTERNAL_ERROR;
            }
            return result;
        }

        public async Task<RpcResult<TResult>> AsyncRequest<T, TResult>(string callName, string groupName, int serviceId, ushort messageId,
            T req, int timeout = 3000)
        {
            RpcResult<TResult> result = new RpcResult<TResult>();
            var reqMessage = AmpMessage.CreateRequestMessage(serviceId, messageId);
            reqMessage.FriendlyServiceName = callName;
            reqMessage.ServiceGroupName = groupName;
            reqMessage.CodecType = (CodecType) Enum.ToObject(typeof(CodecType), this._serializer.CodecType);
            reqMessage.Data = this._serializer.Serialize(req);
            var rsp = await AsyncCallInner(reqMessage, timeout);
            if (rsp != null)
            {
                result.Code = rsp.Code;
                if (rsp.Data != null)
                {
                    result.Data = this._serializer.Deserialize<TResult>(rsp.Data);
                }
            }
            else
            {
                this._logger.LogError("Call {0} , return null", callName);
                result.Code = RpcErrorCodes.CODE_INTERNAL_ERROR;
            }
            return result;
        }

        private async Task<bool> SendAsync(AmpMessage request)
        {
            bool success;
            try
            {
                //发送
                await this._rpcClient.SendAsync(request);
                success = true;
            }
            catch (ClosedChannelException closedEx)
            {
                this._logger.LogError(closedEx, "send message error,channel closed,{messageId}", request.Id);
                TomatoDiagnosticListenerExtensions.Listener.ClientException(request, closedEx);
                throw new RpcCommunicationException($"send message error,channel closed,{request.Id}", closedEx);
            }
            catch (Exception exception)
            {
                success = false;
                ErrorCallBack(request.Id);
                this._logger.LogError(exception, "error occ:");
                TomatoDiagnosticListenerExtensions.Listener.ClientException(request, exception);
            }
            return success;
        }

        private void MessageReceived(AmpMessage message)
        {

            if (message.ServiceId == 0 && message.MessageId == 0)
            {
                this._logger.LogTrace("received heart beat");
                return;
            }

            TaskCompletionSource<AmpMessage> task;
            if (message.Code != 0)
            {
                this._logger.LogDebug("server response error msg ,type={0}", message.InvokeMessageType);
                if (this._resultDictionary.TryRemove(message.Id, out task))
                {
                    task.TrySetResult(message);
                    this._logger.LogDebug("message {0},set result success,message.code ={1}", message.Id, message.Code);
                }
                else
                {
                    this._logger.LogError($"server response error msg ,id={message.Id},code={message.Code},");
                }
            }
            else
            {
                this._logger.LogDebug($"receive message, id:{message.Id}");
                if (this._resultDictionary.TryRemove(message.Id, out task))
                {
                    task.TrySetResult(message);
                    this._logger.LogDebug("message {0},set result success,message.code ={1}", message.Id, message.Code);
                }

            }

        }

        private void TimeOutCallBack(string id)
        {
            if (this._resultDictionary.TryRemove(id, out var task))
            {
                var message = AmpMessage.CreateResponseMessage(id);
                message.Code = RpcErrorCodes.CODE_TIMEOUT;
                if (!task.TrySetResult(message))
                {
                    this._logger.LogWarning("set timeout result fail,maybe task is completed,message {0}", id);
                }

                this._logger.LogWarning("message {0}, timeout", id);
            }
        }

        private void ErrorCallBack(string id)
        {
            if (!this._resultDictionary.ContainsKey(id))
            {
                return;
            }

            if (this._resultDictionary.TryRemove(id, out var task))
            {
                var message = AmpMessage.CreateResponseMessage(id);
                message.Code = RpcErrorCodes.CODE_INTERNAL_ERROR;
                if (!task.TrySetResult(message))
                {
                    this._logger.LogWarning("set error result fail,maybe task is completed");
                }

                this._logger.LogWarning("message {0}, error", id);
            }
        }

        private Task<AmpMessage> RegisterResultCallbackAsync(string id)
        {
            var tcs = new TaskCompletionSource<AmpMessage>();

            this._resultDictionary.TryAdd(id, tcs);
            return tcs.Task;
        }

        private void AutoSetSequence(AmpMessage request)
        {
            int id = Interlocked.Increment(ref INVOKER_SEQ);
            request.Sequence = id;
        }

    }
}
