FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5010

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/BCP.Stub/BCP.Stub.csproj", "src/Services/BCP.Stub/"]
RUN dotnet restore "src/Services/BCP.Stub/BCP.Stub.csproj"
COPY . .
WORKDIR "/src/src/Services/BCP.Stub"
RUN dotnet build "BCP.Stub.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BCP.Stub.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BCP.Stub.dll"]
