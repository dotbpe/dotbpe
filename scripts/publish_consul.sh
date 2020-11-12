set -ex

cd $(dirname $0)/../

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

mkdir -p $artifactsFolder

dotnet build ./src/Tomato.Extra.Consul/Tomato.Extra.Consul.csproj -c Release

dotnet pack ./src/Tomato.Extra.Consul/Tomato.Extra.Consul.csproj -c Release -o $artifactsFolder

dotnet nuget push ./$artifactsFolder/Tomato.Extra.Consul.*.nupkg -k $NUGET_KEY -s https://www.nuget.org
