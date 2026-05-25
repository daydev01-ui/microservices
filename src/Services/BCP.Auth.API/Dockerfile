FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Shared/BCP.Shared/BCP.Shared.csproj", "src/Shared/BCP.Shared/"]
COPY ["src/Services/BCP.Auth.API/BCP.Auth.API.csproj", "src/Services/BCP.Auth.API/"]
RUN dotnet restore "src/Services/BCP.Auth.API/BCP.Auth.API.csproj"
COPY . .
WORKDIR "/src/src/Services/BCP.Auth.API"
RUN dotnet build "BCP.Auth.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BCP.Auth.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BCP.Auth.API.dll"]
