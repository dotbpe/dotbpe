// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: services/account.proto
#region Designer generated code

using System; 
using System.Threading.Tasks; 
using DotBPE.Rpc; 
using DotBPE.Protocol.Amp; 
using DotBPE.Rpc.Exceptions; 
using Google.Protobuf; 

namespace PiggyMetrics.Common {

//start for class AccountServiceClient
public sealed class AccountServiceClient : AmpInvokeClient 
{
public AccountServiceClient(IRpcClient<AmpMessage> client) : base(client)
{
}
public async Task<AccountRsp> FindByNameAsync(FindAccountReq request,int timeOut=3000)
{
AmpMessage message = AmpMessage.CreateRequestMessage(1001, 1);
message.Data = request.ToByteArray();
var response = await base.CallInvoker.AsyncCall(message,timeOut);
if (response == null)
{
throw new RpcException("error,response is null !");
}
if (response.Data == null)
{
return new AccountRsp();
}
return AccountRsp.Parser.ParseFrom(response.Data);
}

//同步方法
public AccountRsp FindByName(FindAccountReq request)
{
AmpMessage message = AmpMessage.CreateRequestMessage(1001, 1);
message.Data = request.ToByteArray();
var response =  base.CallInvoker.BlockingCall(message);
if (response == null)
{
throw new RpcException("error,response is null !");
}
if (response.Data == null)
{
return new AccountRsp();
}
return AccountRsp.Parser.ParseFrom(response.Data);
}
public async Task<AccountRsp> CreateAsync(UserReq request,int timeOut=3000)
{
AmpMessage message = AmpMessage.CreateRequestMessage(1001, 2);
message.Data = request.ToByteArray();
var response = await base.CallInvoker.AsyncCall(message,timeOut);
if (response == null)
{
throw new RpcException("error,response is null !");
}
if (response.Data == null)
{
return new AccountRsp();
}
return AccountRsp.Parser.ParseFrom(response.Data);
}

//同步方法
public AccountRsp Create(UserReq request)
{
AmpMessage message = AmpMessage.CreateRequestMessage(1001, 2);
message.Data = request.ToByteArray();
var response =  base.CallInvoker.BlockingCall(message);
if (response == null)
{
throw new RpcException("error,response is null !");
}
if (response.Data == null)
{
return new AccountRsp();
}
return AccountRsp.Parser.ParseFrom(response.Data);
}
public async Task<VoidRsp> SaveAsync(AccountReq request,int timeOut=3000)
{
AmpMessage message = AmpMessage.CreateRequestMessage(1001, 3);
message.Data = request.ToByteArray();
var response = await base.CallInvoker.AsyncCall(message,timeOut);
if (response == null)
{
throw new RpcException("error,response is null !");
}
if (response.Data == null)
{
return new VoidRsp();
}
return VoidRsp.Parser.ParseFrom(response.Data);
}

//同步方法
public VoidRsp Save(AccountReq request)
{
AmpMessage message = AmpMessage.CreateRequestMessage(1001, 3);
message.Data = request.ToByteArray();
var response =  base.CallInvoker.BlockingCall(message);
if (response == null)
{
throw new RpcException("error,response is null !");
}
if (response.Data == null)
{
return new VoidRsp();
}
return VoidRsp.Parser.ParseFrom(response.Data);
}
}
//end for class AccountServiceClient
}
#endregion