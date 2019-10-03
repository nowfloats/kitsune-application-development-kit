FROM microsoft/dotnet:2.0-runtime
WORKDIR /app

COPY ./bin/Release/netcoreapp2.0/publish .
ENTRYPOINT ["dotnet", "KitsuneWebsiteCrawlerService.dll"]
