# syntax=docker/dockerfile:1.14

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    NUGET_XMLDOC_MODE=skip
WORKDIR /src

COPY ["ServiceDesk.csproj", "."]
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages,sharing=locked \
    dotnet restore "./ServiceDesk.csproj"

COPY . .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages,sharing=locked \
    dotnet publish "./ServiceDesk.csproj" \
    --no-restore \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:DebugType=none \
    /p:DebugSymbols=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled-extra AS final
USER app
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ServiceDesk.dll"]
