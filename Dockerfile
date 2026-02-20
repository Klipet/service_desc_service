FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ServiceDesck.csproj .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore "ServiceDesck.csproj"

COPY . .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish "ServiceDesck.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

USER $APP_UID

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ServiceDesck.dll"]