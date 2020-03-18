set -ex

cd $(dirname $0)/../

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

mkdir -p $artifactsFolder

dotnet restore ./tomato.sln
dotnet build ./tomato.sln -c Release

dotnet pack ./src/Tomato.Baseline/Tomato.Baseline.csproj -c Release -o $artifactsFolder
dotnet pack ./src/Tomato.Extra.Castle/Tomato.Extra.Castle.csproj -c Release -o $artifactsFolder
dotnet pack ./src/Tomato.Extra.Consul/Tomato.Extra.Consul.csproj -c Release -o $artifactsFolder
dotnet pack ./src/Tomato.Extra.JsonNet/Tomato.Extra.JsonNet.csproj -c Release -o $artifactsFolder
dotnet pack ./src/Tomato.Extra.MessagePack/Tomato.Extra.MessagePack.csproj -c Release -o $artifactsFolder
dotnet pack ./src/Tomato.Extra.Protobuf/Tomato.Extra.Protobuf.csproj -c Release -o $artifactsFolder
dotnet pack ./src/Tomato.Gateway/Tomato.Gateway.csproj -c Release -o $artifactsFolder
dotnet pack ./src/Tomato.Gateway.Swagger/Tomato.Gateway.Swagger.csproj -c Release -o $artifactsFolder
dotnet pack ./src/Tomato.Gateway.SwaggerUI/Tomato.Gateway.SwaggerUI.csproj -c Release -o $artifactsFolder
dotnet pack ./src/Tomato.Rpc/Tomato.Rpc.csproj -c Release -o $artifactsFolder
