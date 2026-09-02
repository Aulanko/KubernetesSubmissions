# Build stage - Use .NET 10.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY todo-backend.csproj .
RUN dotnet restore todo-backend.csproj

COPY . .
RUN dotnet publish todo-backend.csproj -c Release -o /app/publish

# Runtime stage - Use ASP.NET Core runtime (includes Microsoft.AspNetCore.App)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=3002

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "todo-backend.dll"]