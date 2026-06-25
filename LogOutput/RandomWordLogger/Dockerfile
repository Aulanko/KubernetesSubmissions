# Build stage - Use .NET 10.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY LogOutput.csproj .
RUN dotnet restore LogOutput.csproj

COPY . .
RUN dotnet publish LogOutput.csproj -c Release -o /app/publish

# Runtime stage - Use ASP.NET Core runtime (includes Microsoft.AspNetCore.App)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "LogOutput.dll"]