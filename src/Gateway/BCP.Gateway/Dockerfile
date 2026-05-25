FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Gateway/BCP.Gateway/BCP.Gateway.csproj", "src/Gateway/BCP.Gateway/"]
RUN dotnet restore "src/Gateway/BCP.Gateway/BCP.Gateway.csproj"
COPY . .
WORKDIR "/src/src/Gateway/BCP.Gateway"
RUN dotnet build "BCP.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BCP.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY src/Gateway/BCP.Gateway/ocelot.json .
ENTRYPOINT ["dotnet", "BCP.Gateway.dll"]
