FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5009
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Shared/BCP.Shared/BCP.Shared.csproj", "src/Shared/BCP.Shared/"]
COPY ["src/Services/BCP.CashClosing.API/BCP.CashClosing.API.csproj", "src/Services/BCP.CashClosing.API/"]
RUN dotnet restore "src/Services/BCP.CashClosing.API/BCP.CashClosing.API.csproj"
COPY . .
WORKDIR "/src/src/Services/BCP.CashClosing.API"
RUN dotnet publish "BCP.CashClosing.API.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BCP.CashClosing.API.dll"]
