set -ex

cd $(dirname $0)/../

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

mkdir -p $artifactsFolder


dotnet build ./src/Tomato.Gateway/Tomato.Gateway.csproj -c Release

dotnet pack ./src/Tomato.Gateway/Tomato.Gateway.csproj -c Release -o $artifactsFolder

dotnet nuget push ./$artifactsFolder/Tomato.Gateway.*.nupkg -k $NUGET_KEY -s https://www.nuget.org
