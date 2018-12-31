using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Peach.Diagnostics;

namespace Peach.Tcp
{   
    public class TcpClientChannelHandlerAdapter<TMessage> : SimpleChannelInboundHandler<TMessage> where TMessage : Messaging.IMessage
    {
        private static DiagnosticListener listener = new DiagnosticListener(Diagnostics.DiagnosticListenerExtensions.DiagnosticListenerName);

        private readonly ISocketClient<TMessage> _client;
        private readonly Protocol.IProtocol<TMessage> _protocol;

        public TcpClientChannelHandlerAdapter(ISocketClient<TMessage> client,Protocol.IProtocol<TMessage> protocol)
        {
            _client = client;
            _protocol = protocol;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            _client.RaiseConnected(new SocketContext<TMessage>(context.Channel, _protocol));
            base.ChannelActive(context);
        }      

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="context"></param>
        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _client.RaiseDisconnected(new SocketContext<TMessage>(context.Channel, _protocol));
            base.ChannelInactive(context);
        }

        protected override void ChannelRead0(IChannelHandlerContext context, TMessage msg)
        {
            listener.ClientReceive(msg);
            _client.RaiseReceive(new SocketContext<TMessage>(context.Channel, _protocol), msg);
            listener.ClientReceiveComplete(msg);
            //this._bootstrap.ChannelRead(ctx, msg);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception ex)
        {
            _client.RaiseError(new SocketContext<TMessage>(context.Channel, _protocol), ex);
            listener.ClientException(ex);
            context.CloseAsync(); //关闭连接
        }

        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            if (evt is IdleStateEvent)
            {
                var eventState = evt as IdleStateEvent;
                if (eventState != null)
                {                  
                    _client.RaiseIdleState(new SocketContext<TMessage>(context.Channel, _protocol), eventState);
                }
            }
        }
    }
}
