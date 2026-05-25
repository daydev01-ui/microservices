FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5004

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Shared/BCP.Shared/BCP.Shared.csproj", "src/Shared/BCP.Shared/"]
COPY ["src/Services/BCP.QR.API/BCP.QR.API.csproj", "src/Services/BCP.QR.API/"]
RUN dotnet restore "src/Services/BCP.QR.API/BCP.QR.API.csproj"
COPY . .
WORKDIR "/src/src/Services/BCP.QR.API"
RUN dotnet build "BCP.QR.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BCP.QR.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BCP.QR.API.dll"]
