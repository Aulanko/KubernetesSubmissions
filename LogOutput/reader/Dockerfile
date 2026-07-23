

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

COPY reader.csproj .
RUN dotnet restore reader.csproj

COPY . .

RUN dotnet publish reader.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=3000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "reader.dll"]