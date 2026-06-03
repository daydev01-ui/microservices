-- =============================================
-- QRPaymentSystem - BCP Bolivia
-- Script: 00_create_database.sql
-- =============================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'BCPQRSystem')
BEGIN
    CREATE DATABASE BCPQRSystem;
END
GO
