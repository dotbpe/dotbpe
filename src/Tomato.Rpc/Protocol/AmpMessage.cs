using Tomato.Rpc.Codec;

namespace Tomato.Rpc.Protocol
{
    /// <summary>
    /// 协议消息
    /// </summary>
    public class AmpMessage : InvokeMessage, Peach.Messaging.IMessage
    {

        public static readonly AmpMessage HEART_BEAT = CreateRequestMessage(0,0,true);

        /// <summary>
        /// 第一个版本为18个字节头固定长度
        /// </summary>
        public const int VERSION_0_HEAD_LENGTH = 18;
        /// <summary>
        /// 加强版本则为21个字节头固定长度
        /// </summary>
        public const int VERSION_1_HEAD_LENGTH = 21;


        /// <summary>
        /// 状态码
        /// </summary>
        public int Code { get; set; }

        //0 默认为Protobuf 1 MessagePack 2 = JSON
        public CodecType CodecType { get; set; }

        /// <summary>
        /// 实际的请求数据
        /// </summary>
        public byte[] Data { get; set; }

        public override bool IsHeartBeat => ServiceId == 0 && MessageId == 0;

        public override int Length {
            get
            {
                var hl = Version == 0 ? VERSION_0_HEAD_LENGTH : VERSION_1_HEAD_LENGTH;
                if(Data == null)
                {
                    return hl;
                }

                return hl + this.Data.Length;
            }
        }

        /// <summary>
        /// 消息标识
        /// </summary>
        public string Id => $"{ServiceId}|{MessageId}|{Sequence}";


        /// <summary>
        /// 调用服务的唯一消息号 确定哪个方法
        /// </summary>
        public ushort MessageId { get; set; }

        public override string MethodIdentifier => $"{ServiceId}.{MessageId}";

        /// <summary>
        /// 请求的序列号
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// 调用服务的唯一服务号 确定哪个服务
        /// </summary>
        public int ServiceId { get; set; }

        public override string ServiceIdentity => $"{ServiceId}.0";

        /// <summary>
        /// 协议版本0/1
        /// </summary>
        public byte Version { get; set; }


        public string FriendlyServiceName { get;set;}
        public string ServiceGroupName { get; set; } ="default";

        public string MessageRoutePath => $"{ServiceGroupName}.{MethodIdentifier}";

        public static AmpMessage CreateRequestMessage(int serviceId,ushort messageId,bool withOutResponse =false)
        {
            return new AmpMessage
            {
                ServiceId = serviceId,
                MessageId = messageId,
                Version = 1,
                CodecType = 0,
                InvokeMessageType =
                    withOutResponse ? InvokeMessageType.InvokeWithoutResponse : InvokeMessageType.Request
            };
        }


        public static AmpMessage CreateResponseMessage(string requestId)
        {
            var data = requestId.Split('|');
            return new AmpMessage
            {
                ServiceId = int.Parse(data[0]),
                MessageId = ushort.Parse(data[1]),
                Version = 1,
                CodecType = 0,
                InvokeMessageType = InvokeMessageType.Response
            };
        }

        public static AmpMessage CreateResponseMessage(int serviceId, ushort messageId)
        {
            return new AmpMessage
            {
                ServiceId = serviceId,
                MessageId = messageId,
                Version = 1,
                CodecType = 0,
                InvokeMessageType = InvokeMessageType.Response
            };
        }
    }
}
