using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Tomato.Gateway;
using Tomato.Rpc;
using Tomato.Rpc.Server;

namespace GreeterHttpService
{
    /// <summary>
    /// Swagger测试服务
    /// </summary>
    [RpcService(102)]
    public interface ISwaggerSampleService
    {

        /// <summary>
        /// 测试方法1GET
        /// </summary>
        /// <param name="req"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [RpcMethod(1)]
        [Router("/api/sample/getSample", RestfulVerb.Get)]
        Task<RpcResult<SampleRes>> GetSampleAsync(SampleReq req, int timeout = 3000);

        /// <summary>
        /// 测试方法2 POST
        /// </summary>
        /// <param name="req"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [RpcMethod(2)]
        [Router("/api/sample/postSample", RestfulVerb.Post, "2.0.0")]
        Task<RpcResult<SampleRes>> PostSampleAsync(SampleReq req, int timeout = 3000);

    }

    public class SwaggerSampleService : BaseService<ISwaggerSampleService>, ISwaggerSampleService
    {
        public Task<RpcResult<SampleRes>> GetSampleAsync(SampleReq req, int timeout = 3000)
        {
            RpcResult<SampleRes> result = new RpcResult<SampleRes>();
            SampleRes res = new SampleRes();
            result.Data = res;

            res.IntVal = 1;
            res.BoolVal = true;
            res.ByteVal = 1;
            res.StringVal = "1";
            res.DateTimeVal = DateTime.Now;
            res.SampleEnum = SampleEnum.E1;
            res.ObjectVal = new SampleInnerObject
            {
                InnerIntVal = 1,
                InnerBoolVal = true,
                InnerByteVal = 1,
                InnerStringVal = "2",
                InnerDateTimeVal = DateTime.Now
            };

            return Task.FromResult(result);
        }

        public Task<RpcResult<SampleRes>> PostSampleAsync(SampleReq req, int timeout = 3000)
        {
            RpcResult<SampleRes> result = new RpcResult<SampleRes>();
            SampleRes res = new SampleRes();
            result.Data = res;

            res.IntVal = 1;
            res.BoolVal = true;
            res.ByteVal = 1;
            res.StringVal = "1";
            res.DateTimeVal = DateTime.Now;
            res.SampleEnum = SampleEnum.E1;
            res.ObjectVal = new SampleInnerObject
            {
                InnerIntVal = 1,
                InnerBoolVal = true,
                InnerByteVal = 1,
                InnerStringVal = "2",
                InnerDateTimeVal = DateTime.Now
            };

            return Task.FromResult(result);
        }
    }

    [DataContract]
    public class SampleReq
    {
        /// <summary>
        /// 测试string
        /// </summary>
        [DataMember(Name = "stringVal", Order = 1)]
        public string StringVal { get; set; }

        /// <summary>
        /// 整数类型
        /// </summary>
        [DataMember(Name = "intVal", Order = 2)]
        public int IntVal { get; set; }
    }

    [DataContract]
    public class SampleRes
    {
        /// <summary>
        /// 测试string
        /// </summary>
        [DataMember(Name = "stringVal", Order = 1)]
        public string StringVal { get; set; }

        /// <summary>
        /// 整数类型
        /// </summary>
        [DataMember(Name = "intVal", Order = 2)]
        public int IntVal { get; set; }

        /// <summary>
        /// byte类型
        /// </summary>
        [DataMember(Name = "byteVal", Order = 3)]
        public byte ByteVal { get; set; }

        /// <summary>
        /// bool类型
        /// </summary>
        [DataMember(Name = "boolVal", Order = 4)]
        public bool BoolVal { get; set; }

        /// <summary>
        /// 时间类型
        /// </summary>
        [DataMember(Name = "dateTimeVal", Order = 5)]
        public DateTime DateTimeVal { get; set; }

        /// <summary>
        /// 枚举类型
        /// </summary>
        [DataMember(Name = "sampleEnum", Order = 6)]
        public SampleEnum SampleEnum { get; set; }

        /// <summary>
        /// 对象值
        /// </summary>
        [DataMember(Name = "objectVal", Order = 7)]
        public SampleInnerObject ObjectVal { get; set; }

        /// <summary>
        /// 列表字符串
        /// </summary>
        /// <value></value>
        [DataMember(Name = "sampleStringList", Order = 8)]
        public List<string> SampleStringList { get; set; }
        /// <summary>
        /// 列表值类型
        /// </summary>
        /// <value></value>
        [DataMember(Name = "sampleIntList", Order = 8)]
        public List<int> SampleIntList { get; set; }
        /// <summary>
        ///  列表对象
        /// </summary>
        /// <value></value>
        [DataMember(Name = "listObjectVals", Order = 8)]
        public List<SampleInnerObject> ListObjectVals { get; set; }
    }

    /// <summary>
    /// 内部的对象
    /// </summary>
    [DataContract]
    public class SampleInnerObject
    {
        /// <summary>
        /// 测试string
        /// </summary>
        [DataMember(Name = "innerStringVal", Order = 1)]
        public string InnerStringVal { get; set; }

        /// <summary>
        /// 内部的整型
        /// </summary>
        [DataMember(Name = "innerIntVal", Order = 2)]
        public int InnerIntVal { get; set; }

        /// <summary>
        /// 内部byte
        /// </summary>
        [DataMember(Name = "innerByteVal", Order = 3)]
        public byte InnerByteVal { get; set; }

        /// <summary>
        /// 内部bool
        /// </summary>
        [DataMember(Name = "innerBoolVal", Order = 4)]
        public bool InnerBoolVal { get; set; }

        /// <summary>
        /// 内部时间
        /// </summary>
        [DataMember(Name = "innerDateTimeVal", Order = 5)]
        public DateTime InnerDateTimeVal { get; set; }

    }

    /// <summary>
    /// 测试枚举值
    /// </summary>
    [DataContract]
    public enum SampleEnum
    {
        /// <summary>
        /// 枚举值E1
        /// </summary>
        E1 = 1,
        /// <summary>
        /// 枚举值E2
        /// </summary>
        E2 = 2
    }
}
