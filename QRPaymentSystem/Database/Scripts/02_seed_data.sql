-- QRPaymentSystem - BCP Bolivia
-- Script 02: Seed Data

USE BCPQRSystem;
GO

-- Insert Roles
IF NOT EXISTS (SELECT 1 FROM Roles WHERE NombreRol = 'Administrador')
INSERT INTO Roles (NombreRol) VALUES ('Administrador'), ('Operador'), ('Supervisor');
GO

-- Insert Empresas
IF NOT EXISTS (SELECT 1 FROM Empresas WHERE NIT = '1234567')
INSERT INTO Empresas (NombreEmpresa, NIT, Direccion, EstadoEmpresa) VALUES
('Empresa Comercial ABC', '1234567', 'Av. Arce 1234, La Paz', 'Activo'),
('Distribuidora XYZ', '7654321', 'Calle Comercio 567, Santa Cruz', 'Activo');
GO

-- Insert Users (password: Test1234 - BCrypt hashed)
-- Hash: $2a$11$rBnNiOPdFPDQyYq7jN8ZZOqBVrCRxI5MWpD3J8RqYnWMkZ6wGDvGi
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE CorreoElectronico = 'admin@bcp.com')
INSERT INTO Usuarios (NombreCompleto, CorreoElectronico, Contraseña, EstadoUsuario, IdRol, IdEmpresa)
VALUES
('Administrador BCP', 'admin@bcp.com', '$2a$11$rBnNiOPdFPDQyYq7jN8ZZOqBVrCRxI5MWpD3J8RqYnWMkZ6wGDvGi', 'Activo',
    (SELECT IdRol FROM Roles WHERE NombreRol = 'Administrador'),
    (SELECT IdEmpresa FROM Empresas WHERE NIT = '1234567')),
('Operador BCP', 'operador@bcp.com', '$2a$11$rBnNiOPdFPDQyYq7jN8ZZOqBVrCRxI5MWpD3J8RqYnWMkZ6wGDvGi', 'Activo',
    (SELECT IdRol FROM Roles WHERE NombreRol = 'Operador'),
    (SELECT IdEmpresa FROM Empresas WHERE NIT = '1234567')),
('Supervisor BCP', 'supervisor@bcp.com', '$2a$11$rBnNiOPdFPDQyYq7jN8ZZOqBVrCRxI5MWpD3J8RqYnWMkZ6wGDvGi', 'Activo',
    (SELECT IdRol FROM Roles WHERE NombreRol = 'Supervisor'),
    (SELECT IdEmpresa FROM Empresas WHERE NIT = '7654321'));
GO
