FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Frontend/BCP.Web/BCP.Web.csproj", "src/Frontend/BCP.Web/"]
RUN dotnet restore "src/Frontend/BCP.Web/BCP.Web.csproj"
COPY . .
WORKDIR "/src/src/Frontend/BCP.Web"
RUN dotnet build "BCP.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BCP.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BCP.Web.dll"]
