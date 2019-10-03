FROM microsoft/dotnet:2.0-runtime
WORKDIR /app
COPY ${source:-obj/Docker/publish} .
ENTRYPOINT ["dotnet", "Kitsune.Compiler.BuildService.dll"]
