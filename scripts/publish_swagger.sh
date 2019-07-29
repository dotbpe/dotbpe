set -ex

cd $(dirname $0)/../

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

mkdir -p $artifactsFolder

dotnet build ./src/Tomato.Gateway.Swagger/Tomato.Gateway.Swagger.csproj -c Release
dotnet pack ./src/Tomato.Gateway.Swagger/Tomato.Gateway.Swagger.csproj -c Release -o ../../$artifactsFolder

dotnet nuget push ./$artifactsFolder/Tomato.Gateway.Swagger.*.nupkg -k $NUGET_KEY -s https://www.nuget.org
