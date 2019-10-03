FROM microsoft/dotnet:2.1.4-aspnetcore-runtime
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

COPY ${source:-obj/Docker/publish} .
ENTRYPOINT ["dotnet", "Kitsune.API2.dll"]

