-- QRPaymentSystem - BCP Bolivia
-- Script 01: Create Tables

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'BCPQRSystem')
BEGIN
    CREATE DATABASE BCPQRSystem;
END
GO

USE BCPQRSystem;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
CREATE TABLE Roles (
    IdRol INT PRIMARY KEY IDENTITY(1,1),
    NombreRol NVARCHAR(50) NOT NULL
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Empresas' AND xtype='U')
CREATE TABLE Empresas (
    IdEmpresa INT PRIMARY KEY IDENTITY(1,1),
    NombreEmpresa NVARCHAR(100) NOT NULL,
    NIT NVARCHAR(20),
    Direccion NVARCHAR(200),
    EstadoEmpresa NVARCHAR(20) DEFAULT 'Activo'
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Usuarios' AND xtype='U')
CREATE TABLE Usuarios (
    IdUsuario INT PRIMARY KEY IDENTITY(1,1),
    NombreCompleto NVARCHAR(100) NOT NULL,
    CorreoElectronico NVARCHAR(100) UNIQUE NOT NULL,
    Contraseña NVARCHAR(255) NOT NULL,
    EstadoUsuario NVARCHAR(20) DEFAULT 'Activo',
    IdRol INT FOREIGN KEY REFERENCES Roles(IdRol),
    IdEmpresa INT FOREIGN KEY REFERENCES Empresas(IdEmpresa)
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CodigosQR' AND xtype='U')
CREATE TABLE CodigosQR (
    IdQR INT PRIMARY KEY IDENTITY(1,1),
    CodigoQR NVARCHAR(100) UNIQUE NOT NULL,
    Monto DECIMAL(10,2) NOT NULL,
    FechaCreacion DATETIME DEFAULT GETDATE(),
    EstadoQR NVARCHAR(20) DEFAULT 'Activo',
    IdEmpresa INT FOREIGN KEY REFERENCES Empresas(IdEmpresa)
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Transacciones' AND xtype='U')
CREATE TABLE Transacciones (
    IdTransaccion INT PRIMARY KEY IDENTITY(1,1),
    FechaTransaccion DATETIME DEFAULT GETDATE(),
    EstadoTransaccion NVARCHAR(20) NOT NULL,
    IdQR INT FOREIGN KEY REFERENCES CodigosQR(IdQR)
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Pagos' AND xtype='U')
CREATE TABLE Pagos (
    IdPago INT PRIMARY KEY IDENTITY(1,1),
    MontoPago DECIMAL(10,2) NOT NULL,
    FechaPago DATETIME DEFAULT GETDATE(),
    MetodoPago NVARCHAR(50) DEFAULT 'QR',
    IdTransaccion INT UNIQUE FOREIGN KEY REFERENCES Transacciones(IdTransaccion)
);
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reportes' AND xtype='U')
CREATE TABLE Reportes (
    IdReporte INT PRIMARY KEY IDENTITY(1,1),
    TipoReporte NVARCHAR(50),
    FechaGeneracion DATETIME DEFAULT GETDATE(),
    IdUsuario INT FOREIGN KEY REFERENCES Usuarios(IdUsuario)
);
GO
