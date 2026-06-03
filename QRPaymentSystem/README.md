# QRPaymentSystem - BCP Bolivia

Sistema de gestión de pagos mediante códigos QR para el Banco de Crédito de Bolivia (BCP).

## Arquitectura

```
┌─────────────────────────────────────────────────────────────┐
│                     Frontend (Blazor Server :5006)          │
└──────────────────────────┬──────────────────────────────────┘
                           │ HTTP
┌──────────────────────────▼──────────────────────────────────┐
│               API Gateway / YARP (:5000)                    │
│  /auth/** → AuthService                                     │
│  /qr/**   → QRService                                       │
│  /transactions/** → TransactionService                      │
│  /reports/** → ReportingService                             │
└───────┬──────────┬──────────────┬─────────────┬────────────┘
        │          │              │             │
   ┌────▼────┐ ┌───▼───┐  ┌──────▼──────┐ ┌───▼──────────┐
   │AuthSvc  │ │QRSvc  │  │Transaction  │ │Reporting     │
   │:5001    │ │:5002  │  │Svc :5003    │ │Svc :5004     │
   └────┬────┘ └───┬───┘  └──────┬──────┘ └───┬──────────┘
        │          │              │             │
        └──────────┴──────────────┴─────────────┘
                           │
               ┌───────────▼───────────┐
               │   SQL Server :1433    │
               │   (BCPQRSystem DB)    │
               └───────────────────────┘
                           │
               ┌───────────▼───────────┐
               │  BcpMockService :5005 │
               │  (Simulador BCP)      │
               └───────────────────────┘
```

## Microservicios

| Servicio          | Puerto | Descripción                              |
|-------------------|--------|------------------------------------------|
| AuthService       | 5001   | Autenticación JWT, usuarios, roles       |
| QRService         | 5002   | Generación y gestión de códigos QR       |
| TransactionService| 5003   | Validación y registro de pagos           |
| ReportingService  | 5004   | Reportes, exportación Excel, SignalR     |
| BcpMockService    | 5005   | Simulador del servicio BCP               |
| ApiGateway        | 5000   | YARP reverse proxy                       |
| Frontend          | 5006   | Blazor Server UI                         |

## Prerrequisitos

- Docker Desktop 4.x+
- Docker Compose v2+
- (Opcional para desarrollo local) .NET 8 SDK, SQL Server 2022

## Ejecutar con Docker Compose

```bash
# Clonar y entrar al directorio
cd QRPaymentSystem

# Iniciar todos los servicios
docker-compose up -d --build

# Ver logs
docker-compose logs -f

# Detener
docker-compose down
```

Acceder a: http://localhost:5006

## Ejecutar Localmente (sin Docker)

### 1. Base de datos
```bash
# SQL Server debe estar en localhost:1433
# Ejecutar scripts:
sqlcmd -S localhost -U sa -P YourPassword123 -i Database/Scripts/01_create_tables.sql
sqlcmd -S localhost -U sa -P YourPassword123 -i Database/Scripts/02_seed_data.sql
```

### 2. Iniciar servicios (en terminales separadas)
```bash
cd Services/BcpMockService && dotnet run
cd Services/AuthService && dotnet run
cd Services/QRService && dotnet run
cd Services/TransactionService && dotnet run
cd Services/ReportingService && dotnet run
cd ApiGateway && dotnet run
cd Frontend && dotnet run
```

## Credenciales de Prueba

| Usuario              | Contraseña | Rol           |
|----------------------|------------|---------------|
| admin@bcp.com        | Test1234   | Administrador |
| operador@bcp.com     | Test1234   | Operador      |
| supervisor@bcp.com   | Test1234   | Supervisor    |

## API - Documentación

### AuthService (http://localhost:5001/swagger)

| Método | Endpoint              | Descripción         |
|--------|-----------------------|---------------------|
| POST   | /api/auth/login       | Iniciar sesión      |
| GET    | /api/users            | Listar usuarios     |
| POST   | /api/users            | Crear usuario       |
| PUT    | /api/users/{id}       | Actualizar usuario  |
| DELETE | /api/users/{id}       | Desactivar usuario  |
| POST   | /api/roles/assign     | Asignar rol         |

### QRService (http://localhost:5002/swagger)

| Método | Endpoint              | Descripción              |
|--------|-----------------------|--------------------------|
| POST   | /api/qr/generate      | Generar código QR        |
| GET    | /api/qr               | Listar QR codes          |
| GET    | /api/qr/{id}          | Detalle de QR            |
| PUT    | /api/qr/{id}/status   | Actualizar estado QR     |

### TransactionService (http://localhost:5003/swagger)

| Método | Endpoint                      | Descripción                 |
|--------|-------------------------------|-----------------------------|
| POST   | /api/transactions/validate    | Validar pago QR             |
| GET    | /api/transactions             | Listar transacciones        |
| GET    | /api/transactions/{id}        | Detalle de transacción      |

### ReportingService (http://localhost:5004/swagger)

| Método | Endpoint                         | Descripción             |
|--------|----------------------------------|-------------------------|
| GET    | /api/reports                     | Reporte consolidado     |
| GET    | /api/reports/export?format=excel | Exportar a Excel        |
| GET    | /api/reports/realtime            | Estadísticas en tiempo real |

### BcpMockService (http://localhost:5005/swagger)

| Método | Endpoint          | Descripción           |
|--------|-------------------|-----------------------|
| POST   | /api/bcp/validate | Validar pago con BCP  |

## JWT Configuration

- Issuer: BCPAuthService
- Audience: BCPQRSystem
- Expiration: 60 minutos
- Algorithm: HS256

## Tecnologías

- .NET 8 / ASP.NET Core
- Blazor Server
- YARP (Yet Another Reverse Proxy)
- Dapper + SQL Server
- BCrypt.Net-Next
- QRCoder
- ClosedXML (Excel)
- SignalR
- Bootstrap 5
- Docker / Docker Compose
