# Runtime image
FROM microsoft/aspnetcore:2.0
WORKDIR /app
EXPOSE 80
COPY "bin/Release/PublishOutput" .
ENTRYPOINT ["dotnet", "KitsuneAdminDashboard.Web.dll"]