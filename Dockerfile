FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish WebsiteComputer.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# Thêm dòng này nếu appsettings.json không nằm trong publish output
COPY --from=build /src/appsettings.json .
ENTRYPOINT ["dotnet", "WebsiteComputer.dll"]
