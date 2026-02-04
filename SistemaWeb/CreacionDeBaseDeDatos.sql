-- 1. CREACIÓN DE LA BASE DE DATOS
IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'SistemaInclusivoDB')
BEGIN
    CREATE DATABASE SistemaInclusivoDB;
END
GO

USE SistemaInclusivoDB;
GO

-- 2. ELIMINAR TABLAS SI EXISTEN (Para reiniciar limpio si es necesario re-ejecutar)
-- Orden inverso por las FK
IF OBJECT_ID('Auditoria_Actividades', 'U') IS NOT NULL DROP TABLE Auditoria_Actividades;
IF OBJECT_ID('Actividades', 'U') IS NOT NULL DROP TABLE Actividades;
IF OBJECT_ID('Usuarios', 'U') IS NOT NULL DROP TABLE Usuarios;
IF OBJECT_ID('Cat_Generos', 'U') IS NOT NULL DROP TABLE Cat_Generos;
IF OBJECT_ID('Cat_Discapacidades', 'U') IS NOT NULL DROP TABLE Cat_Discapacidades;
IF OBJECT_ID('Cat_Estados', 'U') IS NOT NULL DROP TABLE Cat_Estados;
GO

-- 3. CREACIÓN DE TABLAS

-- A) Catálogos
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

-- B) Usuarios
CREATE TABLE Usuarios (
    IdUsuario NVARCHAR(15) PRIMARY KEY, -- Cédula
    Correo NVARCHAR(100) NOT NULL UNIQUE,
    Clave NVARCHAR(100) NOT NULL, 
    Nombre NVARCHAR(100) NOT NULL,
    Rol NVARCHAR(50) NOT NULL CHECK (Rol IN ('Administrador', 'Profesor', 'Estudiante')),
    IdGenero INT NULL,
    CONSTRAINT FK_Usuarios_Generos FOREIGN KEY (IdGenero) REFERENCES Cat_Generos(IdGenero)
);

-- C) Actividades
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

-- D) Auditoría
CREATE TABLE Auditoria_Actividades (
    IdAuditoria INT IDENTITY(1,1) PRIMARY KEY,
    CodigoActividad NVARCHAR(20),
    Accion NVARCHAR(10),
    FechaAccion DATETIME DEFAULT GETDATE(),
    IdUsuario NVARCHAR(15) NULL,
    Detalle NVARCHAR(MAX),
    CONSTRAINT FK_Auditoria_Usuarios FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario)
);
GO

-- 4. DATOS SEMILLA (INSERTS)

INSERT INTO Cat_Estados (NombreEstado) VALUES ('Activo'), ('Inactivo'), ('Finalizado'), ('Lleno');
INSERT INTO Cat_Discapacidades (NombreDiscapacidad) VALUES ('Motriz'), ('Visual'), ('Auditiva'), ('Intelectual'), ('Ninguna');
INSERT INTO Cat_Generos (IdGenero, NombreGenero) VALUES (0, 'Masculino'), (1, 'Femenino'), (2, 'No Binario');

-- Usuarios de Prueba
INSERT INTO Usuarios (IdUsuario, Correo, Clave, Nombre, Rol, IdGenero) 
VALUES ('1700000000', 'admin@uisek.edu.ec', '12345', 'Super Admin', 'Administrador', 0);

INSERT INTO Usuarios (IdUsuario, Correo, Clave, Nombre, Rol, IdGenero) 
VALUES ('1700000001', 'juan.perez@uisek.edu.ec', '12345', 'Juan Pérez', 'Profesor', 0);
GO

-- 5. STORED PROCEDURES (CORREGIDOS)

-- A) sp_ObtenerActividades
IF OBJECT_ID('sp_ObtenerActividades', 'P') IS NOT NULL DROP PROCEDURE sp_ObtenerActividades;
GO
CREATE PROCEDURE sp_ObtenerActividades AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        A.Codigo, 
        A.Nombre, 
        A.FechaRealizacion, 
        A.Cupo, 
        U.Nombre AS Responsable, 
        U.Correo AS GmailProfesor,
        A.Latitud, 
        A.Longitud,
        E.NombreEstado AS Estado, 
        D.NombreDiscapacidad AS TipoDiscapacidad 
    FROM Actividades A
    INNER JOIN Usuarios U ON A.IdResponsable = U.IdUsuario
    INNER JOIN Cat_Estados E ON A.IdEstado = E.IdEstado
    INNER JOIN Cat_Discapacidades D ON A.IdDiscapacidad = D.IdDiscapacidad;
END;
GO

-- B) sp_ObtenerActividadPorId
IF OBJECT_ID('sp_ObtenerActividadPorId', 'P') IS NOT NULL DROP PROCEDURE sp_ObtenerActividadPorId;
GO
CREATE PROCEDURE sp_ObtenerActividadPorId 
    @Codigo NVARCHAR(20) 
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        A.Codigo, A.Nombre, A.FechaRealizacion, A.Cupo, 
        U.Nombre AS Responsable, 
        U.Correo AS GmailProfesor, 
        A.Latitud, A.Longitud,
        E.NombreEstado AS Estado, 
        D.NombreDiscapacidad AS TipoDiscapacidad
    FROM Actividades A
    INNER JOIN Usuarios U ON A.IdResponsable = U.IdUsuario
    INNER JOIN Cat_Estados E ON A.IdEstado = E.IdEstado
    INNER JOIN Cat_Discapacidades D ON A.IdDiscapacidad = D.IdDiscapacidad
    WHERE A.Codigo = @Codigo;
END;
GO

-- C) sp_InsertarActividad
IF OBJECT_ID('sp_InsertarActividad', 'P') IS NOT NULL DROP PROCEDURE sp_InsertarActividad;
GO
CREATE PROCEDURE sp_InsertarActividad
    @Codigo NVARCHAR(20),
    @Nombre NVARCHAR(100),
    @FechaRealizacion DATETIME,
    @Cupo INT,
    @IdResponsable NVARCHAR(15),
    @Latitud FLOAT,
    @Longitud FLOAT,
    @NombreEstado NVARCHAR(50),
    @NombreDiscapacidad NVARCHAR(100),
    @IdUsuarioAuditoria NVARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DECLARE @IdEstado INT = (SELECT TOP 1 IdEstado FROM Cat_Estados WHERE NombreEstado = @NombreEstado);
        DECLARE @IdDiscapacidad INT = (SELECT TOP 1 IdDiscapacidad FROM Cat_Discapacidades WHERE NombreDiscapacidad = @NombreDiscapacidad);
        
        -- Valores por defecto si no existen
        IF @IdEstado IS NULL SET @IdEstado = 1; 
        IF @IdDiscapacidad IS NULL SET @IdDiscapacidad = 5; 

        INSERT INTO Actividades (Codigo, Nombre, FechaRealizacion, Cupo, IdResponsable, Latitud, Longitud, IdEstado, IdDiscapacidad)
        VALUES (@Codigo, @Nombre, @FechaRealizacion, @Cupo, @IdResponsable, @Latitud, @Longitud, @IdEstado, @IdDiscapacidad);

        -- Auditoría
        DECLARE @NombreRespLog NVARCHAR(100) = (SELECT Nombre FROM Usuarios WHERE IdUsuario = @IdResponsable);
        INSERT INTO Auditoria_Actividades (CodigoActividad, Accion, IdUsuario, Detalle)
        VALUES (@Codigo, 'INSERT', @IdUsuarioAuditoria, CONCAT('Actividad creada: ', @Nombre, '. Asignada a: ', @NombreRespLog));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- D) sp_ActualizarActividad
IF OBJECT_ID('sp_ActualizarActividad', 'P') IS NOT NULL DROP PROCEDURE sp_ActualizarActividad;
GO
CREATE PROCEDURE sp_ActualizarActividad
    @Codigo NVARCHAR(20),
    @Nombre NVARCHAR(100),
    @FechaRealizacion DATETIME,
    @Cupo INT,
    @IdResponsable NVARCHAR(15), 
    @Latitud FLOAT,
    @Longitud FLOAT,
    @NombreEstado NVARCHAR(50),
    @NombreDiscapacidad NVARCHAR(100),
    @IdUsuarioAuditoria NVARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @IdEstado INT = (SELECT TOP 1 IdEstado FROM Cat_Estados WHERE NombreEstado = @NombreEstado);
        DECLARE @IdDiscapacidad INT = (SELECT TOP 1 IdDiscapacidad FROM Cat_Discapacidades WHERE NombreDiscapacidad = @NombreDiscapacidad);
        IF @IdEstado IS NULL SET @IdEstado = 1; IF @IdDiscapacidad IS NULL SET @IdDiscapacidad = 5;

        UPDATE Actividades
        SET Nombre = @Nombre, FechaRealizacion = @FechaRealizacion, Cupo = @Cupo,
            IdResponsable = @IdResponsable,
            Latitud = @Latitud, Longitud = @Longitud,
            IdEstado = @IdEstado, IdDiscapacidad = @IdDiscapacidad  
        WHERE Codigo = @Codigo;

        INSERT INTO Auditoria_Actividades (CodigoActividad, Accion, IdUsuario, Detalle)
        VALUES (@Codigo, 'UPDATE', @IdUsuarioAuditoria, CONCAT('Actividad actualizada: ', @Nombre));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- E) sp_EliminarActividad (EL QUE DABA ERROR)
IF OBJECT_ID('sp_EliminarActividad', 'P') IS NOT NULL DROP PROCEDURE sp_EliminarActividad;
GO
CREATE PROCEDURE sp_EliminarActividad
    @Codigo NVARCHAR(20),
    @IdUsuarioAuditoria NVARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF EXISTS (SELECT 1 FROM Actividades WHERE Codigo = @Codigo)
        BEGIN
            DECLARE @NombreBorrado NVARCHAR(100) = (SELECT Nombre FROM Actividades WHERE Codigo = @Codigo);
            
            DELETE FROM Actividades WHERE Codigo = @Codigo;

            INSERT INTO Auditoria_Actividades (CodigoActividad, Accion, IdUsuario, Detalle)
            VALUES (@Codigo, 'DELETE', @IdUsuarioAuditoria, CONCAT('ELIMINADA: ', @NombreBorrado));
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO