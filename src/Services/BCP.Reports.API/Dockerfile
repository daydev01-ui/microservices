FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5007
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Shared/BCP.Shared/BCP.Shared.csproj", "src/Shared/BCP.Shared/"]
COPY ["src/Services/BCP.Reports.API/BCP.Reports.API.csproj", "src/Services/BCP.Reports.API/"]
RUN dotnet restore "src/Services/BCP.Reports.API/BCP.Reports.API.csproj"
COPY . .
WORKDIR "/src/src/Services/BCP.Reports.API"
RUN dotnet publish "BCP.Reports.API.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BCP.Reports.API.dll"]
