FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5008
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Shared/BCP.Shared/BCP.Shared.csproj", "src/Shared/BCP.Shared/"]
COPY ["src/Services/BCP.Notifications.API/BCP.Notifications.API.csproj", "src/Services/BCP.Notifications.API/"]
RUN dotnet restore "src/Services/BCP.Notifications.API/BCP.Notifications.API.csproj"
COPY . .
WORKDIR "/src/src/Services/BCP.Notifications.API"
RUN dotnet publish "BCP.Notifications.API.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BCP.Notifications.API.dll"]
