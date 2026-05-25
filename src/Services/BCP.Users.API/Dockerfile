FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Shared/BCP.Shared/BCP.Shared.csproj", "src/Shared/BCP.Shared/"]
COPY ["src/Services/BCP.Users.API/BCP.Users.API.csproj", "src/Services/BCP.Users.API/"]
RUN dotnet restore "src/Services/BCP.Users.API/BCP.Users.API.csproj"
COPY . .
WORKDIR "/src/src/Services/BCP.Users.API"
RUN dotnet build "BCP.Users.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BCP.Users.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BCP.Users.API.dll"]
