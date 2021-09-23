

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
# NewRelic Start
# Add NewRelic agent
RUN apt-get update && \
   apt-get install -y gnupg && \
   echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list && \
   curl -L https://download.newrelic.com/548C16BF.gpg | apt-key add - && \
   apt-get update && \
   apt-get install -y newrelic-netcore20-agent && \
   rm -rf /var/lib/apt/lists/*

ENV CORECLR_ENABLE_PROFILING=1 \
CORECLR_PROFILER=[[KIT_CLOUD_CORECLR_PROFILER]] \
CORECLR_NEWRELIC_HOME=/usr/local/newrelic-netcore20-agent \
CORECLR_PROFILER_PATH=/usr/local/newrelic-netcore20-agent/libNewRelicProfiler.so \
NEW_RELIC_LICENSE_KEY=[[KIT_CLOUD_NEW_RELIC_KEY]] \
NEW_RELIC_APP_NAME=Kitsune-API2-Docker

## NewRelic End

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Kitsune.API2/Kitsune.API2.csproj", "Kitsune.API2/"]
COPY ["KitsunePlugins/Kitsune.BasePlugin/Kitsune.BasePlugin.csproj", "KitsunePlugins/Kitsune.BasePlugin/"]
COPY ["Common/Kitsune.Models/Kitsune.Models.csproj", "Common/Kitsune.Models/"]
COPY ["Common/Kitsune.Language/Kitsune.Language.csproj", "Common/Kitsune.Language/"]
COPY ["Common/Kitsune.Constants/Kitsune.Constants.csproj", "Common/Kitsune.Constants/"]
COPY ["Kitsune.API.Model/Kitsune.API.Model.csproj", "Kitsune.API.Model/"]
COPY ["Compiler/Kitsune.Compiler/Kitsune.Compiler.csproj", "Compiler/Kitsune.Compiler/"]
COPY ["Common/Kitsune.SyntaxParser/Kitsune.SyntaxParser.csproj", "Common/Kitsune.SyntaxParser/"]
COPY ["AntlrLibrary/AntlrLibrary.csproj", "AntlrLibrary/"]
COPY ["Common/Kitsune.Helper/Kitsune.Helper.csproj", "Common/Kitsune.Helper/"]
COPY ["AmazonAWSHelpers/AmazonAWSHelpers.csproj", "AmazonAWSHelpers/"]
COPY ["AWS.Services/AWS.Services.csproj", "AWS.Services/"]
COPY nuget.config ./
RUN dotnet restore "Kitsune.API2/Kitsune.API2.csproj" -s https://nuget.withfloats.com/api/v2 -s https://api.nuget.org/v3/index.json -s https://nuget.pkg.github.com/nowfloats/index.json  --configfile nuget.config
COPY . .
WORKDIR "/src/Kitsune.API2"
RUN dotnet build "Kitsune.API2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Kitsune.API2.csproj" -c Release -o /app/publish --source https://nuget.withfloats.com/api/v2 --source https://api.nuget.org/v3/index.json --source https://nuget.pkg.github.com/nowfloats/index.json

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kitsune.API2.dll"]
