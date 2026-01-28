# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CsvImportApp.csproj", "."]
RUN dotnet restore "CsvImportApp.csproj"

COPY . .
RUN dotnet build "CsvImportApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CsvImportApp.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_HTTPS_PORTS=8081

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY entrypoint.sh /app/

RUN chmod +x /app/entrypoint.sh

ENTRYPOINT ["/app/entrypoint.sh"]
