-- =============================================
-- 1. CONFIGURACIÓN DE LA BASE DE DATOS
-- =============================================
USE master;
GO

IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'SistemaInclusivoDBDiagrama')
BEGIN
    CREATE DATABASE SistemaInclusivoDBDiagrama;
END
GO

USE SistemaInclusivoDBDiagrama;
GO

-- =============================================
-- 2. LIMPIEZA DE OBJETOS (Orden inverso para evitar errores de FK)
-- =============================================
-- Procedimientos
IF OBJECT_ID('sp_ObtenerActividades', 'P') IS NOT NULL DROP PROCEDURE sp_ObtenerActividades;
IF OBJECT_ID('sp_ObtenerActividadPorId', 'P') IS NOT NULL DROP PROCEDURE sp_ObtenerActividadPorId;
IF OBJECT_ID('sp_InsertarActividad', 'P') IS NOT NULL DROP PROCEDURE sp_InsertarActividad;
IF OBJECT_ID('sp_ActualizarActividad', 'P') IS NOT NULL DROP PROCEDURE sp_ActualizarActividad;
IF OBJECT_ID('sp_EliminarActividad', 'P') IS NOT NULL DROP PROCEDURE sp_EliminarActividad;

-- Trigger
IF OBJECT_ID('trg_Auditoria_Actividades', 'TR') IS NOT NULL DROP TRIGGER trg_Auditoria_Actividades;

-- Tablas
IF OBJECT_ID('Auditoria_Actividades', 'U') IS NOT NULL DROP TABLE Auditoria_Actividades;
IF OBJECT_ID('Actividades', 'U') IS NOT NULL DROP TABLE Actividades;
IF OBJECT_ID('Usuarios', 'U') IS NOT NULL DROP TABLE Usuarios;
IF OBJECT_ID('Cat_Roles', 'U') IS NOT NULL DROP TABLE Cat_Roles;
IF OBJECT_ID('Cat_Generos', 'U') IS NOT NULL DROP TABLE Cat_Generos;
IF OBJECT_ID('Cat_Discapacidades', 'U') IS NOT NULL DROP TABLE Cat_Discapacidades;
IF OBJECT_ID('Cat_Estados', 'U') IS NOT NULL DROP TABLE Cat_Estados;
GO

-- =============================================
-- 3. CREACIÓN DE TABLAS (Normalización Completa)
-- =============================================

CREATE TABLE Cat_Estados (
    IdEstado INT IDENTITY(1,1) PRIMARY KEY,
    NombreEstado NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Cat_Discapacidades (
    IdDiscapacidad INT IDENTITY(1,1) PRIMARY KEY,
    NombreDiscapacidad NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE Cat_Generos (
    IdGenero INT PRIMARY KEY,
    NombreGenero NVARCHAR(20) NOT NULL
);

CREATE TABLE Cat_Roles (
    IdRol INT PRIMARY KEY,
    NombreRol NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Usuarios (
    IdUsuario NVARCHAR(15) PRIMARY KEY, -- Cédula
    Correo NVARCHAR(100) NOT NULL UNIQUE,
    Clave NVARCHAR(100) NOT NULL, 
    Nombre NVARCHAR(100) NOT NULL,
    IdRol INT NOT NULL, -- FK a Cat_Roles
    IdGenero INT NULL,
    
    CONSTRAINT FK_Usuarios_Roles FOREIGN KEY (IdRol) REFERENCES Cat_Roles(IdRol),
    CONSTRAINT FK_Usuarios_Generos FOREIGN KEY (IdGenero) REFERENCES Cat_Generos(IdGenero)
);

CREATE TABLE Actividades (
    Codigo NVARCHAR(20) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    FechaRealizacion DATETIME NOT NULL,
    Cupo INT NOT NULL CHECK (Cupo > 0),
    IdResponsable NVARCHAR(15) NOT NULL, -- FK Profesor
    Latitud FLOAT NOT NULL DEFAULT -0.09106038310321836,
    Longitud FLOAT NOT NULL DEFAULT -78.4838308629782,
    IdEstado INT NOT NULL,
    IdDiscapacidad INT NOT NULL,

    CONSTRAINT FK_Actividades_Usuarios FOREIGN KEY (IdResponsable) REFERENCES Usuarios(IdUsuario),
    CONSTRAINT FK_Actividades_Estados FOREIGN KEY (IdEstado) REFERENCES Cat_Estados(IdEstado),
    CONSTRAINT FK_Actividades_Discapacidad FOREIGN KEY (IdDiscapacidad) REFERENCES Cat_Discapacidades(IdDiscapacidad)
);

CREATE TABLE Auditoria_Actividades (
    IdAuditoria INT IDENTITY(1,1) PRIMARY KEY,
    CodigoActividad NVARCHAR(20),
    Accion NVARCHAR(20),
    FechaAccion DATETIME DEFAULT GETDATE(),
    IdUsuario NVARCHAR(15) NOT NULL, -- FK Real a Usuarios
    Detalle NVARCHAR(100), -- Mensaje normalizado 
    
    CONSTRAINT FK_Auditoria_Usuarios FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario)
);
GO

-- =============================================
-- 4. CARGA DE DATOS SEMILLA
-- =============================================
INSERT INTO Cat_Estados VALUES ('Activo'), ('Inactivo'), ('Finalizado'), ('Lleno');
INSERT INTO Cat_Discapacidades VALUES ('Motriz'), ('Visual'), ('Auditiva'), ('Intelectual'), ('Ninguna');
INSERT INTO Cat_Generos VALUES (0, 'Masculino'), (1, 'Femenino'), (2, 'No Binario');
INSERT INTO Cat_Roles VALUES (1, 'Administrador'), (2, 'Profesor'), (3, 'Estudiante'), (4, 'Visitante');


INSERT INTO Usuarios (IdUsuario, Correo, Clave, Nombre, IdRol, IdGenero) 
VALUES ('1700000000', 'admin@uisek.edu.ec', '12345', 'Super Admin', 1, 0);

INSERT INTO Usuarios (IdUsuario, Correo, Clave, Nombre, IdRol, IdGenero) 
VALUES ('1700000001', 'juan.perez@uisek.edu.ec', '12345', 'Juan Pérez', 2, 0);

-- IMPORTANTE: Usuario 'fantasma' para el Trigger
INSERT INTO Usuarios (IdUsuario, Correo, Clave, Nombre, IdRol, IdGenero) 
VALUES ('SYSTEM_TRG', 'system@audit.internal', 'no_access', 'System Audit', 1, 0);
GO

-- =============================================
-- 5. TRIGGER DE AUDITORÍA 
-- =============================================
CREATE TRIGGER trg_Auditoria_Actividades
ON Actividades
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Accion NVARCHAR(20);
    DECLARE @Codigo NVARCHAR(20);
    DECLARE @Detalle NVARCHAR(100);
    
    
    DECLARE @IdUsuarioAuditoria NVARCHAR(15) = CAST(SESSION_CONTEXT(N'IdUsuarioAuditoria') AS NVARCHAR(15));

    
    IF @IdUsuarioAuditoria IS NULL OR NOT EXISTS (SELECT 1 FROM Usuarios WHERE IdUsuario = @IdUsuarioAuditoria)
    BEGIN
        SET @IdUsuarioAuditoria = 'SYSTEM_TRG';
    END

   
    IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
    BEGIN
        SET @Accion = 'UPDATE';
        SELECT @Codigo = Codigo FROM inserted;
        SET @Detalle = 'Actualización de registro existente';
    END
    ELSE IF EXISTS (SELECT * FROM inserted)
    BEGIN
        SET @Accion = 'INSERT';
        SELECT @Codigo = Codigo FROM inserted;
        SET @Detalle = 'Creación de nuevo registro';
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        SET @Accion = 'DELETE';
        SELECT @Codigo = Codigo FROM deleted;
        SET @Detalle = 'Eliminación física de registro';
    END
    ELSE RETURN; -- Si no pasó nada, salimos.

    
    INSERT INTO Auditoria_Actividades (CodigoActividad, Accion, IdUsuario, Detalle)
    VALUES (@Codigo, @Accion, @IdUsuarioAuditoria, @Detalle);
END;
GO

-- =============================================
-- 6. PROCEDIMIENTOS ALMACENADOS 
-- =============================================

-- A) OBTENER TODOS
CREATE PROCEDURE sp_ObtenerActividades AS
BEGIN
    SELECT A.Codigo, A.Nombre, A.FechaRealizacion, A.Cupo, U.Nombre AS Responsable, 
           U.Correo AS GmailProfesor, A.Latitud, A.Longitud, E.NombreEstado AS Estado, 
           D.NombreDiscapacidad AS TipoDiscapacidad 
    FROM Actividades A
    JOIN Usuarios U ON A.IdResponsable = U.IdUsuario
    JOIN Cat_Estados E ON A.IdEstado = E.IdEstado
    JOIN Cat_Discapacidades D ON A.IdDiscapacidad = D.IdDiscapacidad;
END;
GO

-- B) OBTENER POR ID
CREATE PROCEDURE sp_ObtenerActividadPorId @Codigo NVARCHAR(20) AS
BEGIN
    SELECT A.Codigo, A.Nombre, A.FechaRealizacion, A.Cupo, U.Nombre AS Responsable, 
           U.Correo AS GmailProfesor, A.Latitud, A.Longitud, E.NombreEstado AS Estado, 
           D.NombreDiscapacidad AS TipoDiscapacidad
    FROM Actividades A
    JOIN Usuarios U ON A.IdResponsable = U.IdUsuario
    JOIN Cat_Estados E ON A.IdEstado = E.IdEstado
    JOIN Cat_Discapacidades D ON A.IdDiscapacidad = D.IdDiscapacidad
    WHERE A.Codigo = @Codigo;
END;
GO

-- C) INSERTAR 
CREATE PROCEDURE sp_InsertarActividad
    @Codigo NVARCHAR(20), @Nombre NVARCHAR(100), @FechaRealizacion DATETIME, @Cupo INT,
    @IdResponsable NVARCHAR(15), @Latitud FLOAT, @Longitud FLOAT, 
    @NombreEstado NVARCHAR(50), @NombreDiscapacidad NVARCHAR(100), 
    @IdUsuarioAuditoria NVARCHAR(15)
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;
        
      
        EXEC sp_set_session_context 'IdUsuarioAuditoria', @IdUsuarioAuditoria;

        DECLARE @IdE INT = (SELECT IdEstado FROM Cat_Estados WHERE NombreEstado = @NombreEstado);
        DECLARE @IdD INT = (SELECT IdDiscapacidad FROM Cat_Discapacidades WHERE NombreDiscapacidad = @NombreDiscapacidad);
        
        INSERT INTO Actividades (Codigo, Nombre, FechaRealizacion, Cupo, IdResponsable, Latitud, Longitud, IdEstado, IdDiscapacidad)
        VALUES (@Codigo, @Nombre, @FechaRealizacion, @Cupo, @IdResponsable, @Latitud, @Longitud, ISNULL(@IdE,1), ISNULL(@IdD,5));
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH 
        ROLLBACK TRANSACTION; 
        THROW; 
    END CATCH
END;
GO

-- D) ACTUALIZAR 
CREATE PROCEDURE sp_ActualizarActividad
    @Codigo NVARCHAR(20), @Nombre NVARCHAR(100), @FechaRealizacion DATETIME, @Cupo INT,
    @IdResponsable NVARCHAR(15), @Latitud FLOAT, @Longitud FLOAT, 
    @NombreEstado NVARCHAR(50), @NombreDiscapacidad NVARCHAR(100), 
    @IdUsuarioAuditoria NVARCHAR(15)
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;
        
        EXEC sp_set_session_context 'IdUsuarioAuditoria', @IdUsuarioAuditoria;

        DECLARE @IdE INT = (SELECT IdEstado FROM Cat_Estados WHERE NombreEstado = @NombreEstado);
        DECLARE @IdD INT = (SELECT IdDiscapacidad FROM Cat_Discapacidades WHERE NombreDiscapacidad = @NombreDiscapacidad);
        
        UPDATE Actividades SET Nombre=@Nombre, FechaRealizacion=@FechaRealizacion, Cupo=@Cupo, IdResponsable=@IdResponsable,
                               Latitud=@Latitud, Longitud=@Longitud, IdEstado=ISNULL(@IdE,1), IdDiscapacidad=ISNULL(@IdD,5)
        WHERE Codigo = @Codigo;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH 
        ROLLBACK TRANSACTION; 
        THROW; 
    END CATCH
END;
GO

-- E) ELIMINAR 
CREATE PROCEDURE sp_EliminarActividad 
    @Codigo NVARCHAR(20), 
    @IdUsuarioAuditoria NVARCHAR(15)
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;
        
        EXEC sp_set_session_context 'IdUsuarioAuditoria', @IdUsuarioAuditoria;

        DELETE FROM Actividades WHERE Codigo = @Codigo;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH 
        ROLLBACK TRANSACTION; 
        THROW; 
    END CATCH
END;
GO