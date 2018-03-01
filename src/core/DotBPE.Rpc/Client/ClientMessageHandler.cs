using DotBPE.Rpc.Codes;
using System;
using System.Threading.Tasks;

namespace DotBPE.Rpc.Client
{
    public class ClientMessageHandler<TMessage> : IClientMessageHandler<TMessage> where TMessage : InvokeMessage
    {
        public event EventHandler<MessageRecievedEventArgs<TMessage>> Recieved;

        public Task ReceiveAsync(IRpcContext<TMessage> context, TMessage message)
        {
            if (Recieved != null)
            {
                return Task.Factory.StartNew(() =>
                {
                    Recieved.Invoke(this, new MessageRecievedEventArgs<TMessage>(context, message));
                }
                );
            }
            else
            {
                return Utils.TaskUtils.CompletedTask;
            }
        }
    }
}
