namespace DotBPE.Protocol.Amp
{
    public class ErrorCodes
    {
        public const int CODE_INTERNAL_ERROR = -10242500; //内部错误
        /// <summary>
        /// 拒绝访问
        /// </summary>
        public const int CODE_ACCESS_DENIED = -10242403;
        public const int CODE_SERVICE_NOT_FOUND = -10242404; // 服务未找到


        public const int CODE_RATELIMITED = -10242409; // 访问受限

        public const int CODE_TIMEOUT = -10242504; // 超时调用
    }
}
