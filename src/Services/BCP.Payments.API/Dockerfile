FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5005

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Shared/BCP.Shared/BCP.Shared.csproj", "src/Shared/BCP.Shared/"]
COPY ["src/Services/BCP.Payments.API/BCP.Payments.API.csproj", "src/Services/BCP.Payments.API/"]
RUN dotnet restore "src/Services/BCP.Payments.API/BCP.Payments.API.csproj"
COPY . .
WORKDIR "/src/src/Services/BCP.Payments.API"
RUN dotnet build "BCP.Payments.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BCP.Payments.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BCP.Payments.API.dll"]
