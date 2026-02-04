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

-- 5. STORED PROCEDURES

-- =============================================
--CREACIÓN DEL TRIGGER DE AUDITORÍA 
-- =============================================
CREATE OR ALTER TRIGGER trg_Auditoria_Actividades
ON Actividades
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Accion NVARCHAR(10);
    DECLARE @Detalle NVARCHAR(MAX);
    DECLARE @Codigo NVARCHAR(20);
    DECLARE @UsuarioSistema NVARCHAR(50) = SYSTEM_USER; 

    -- 1. DETECTAR LA ACCIÓN (Insert, Update, Delete)
    IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
    BEGIN
        SET @Accion = 'UPDATE';
        SELECT @Codigo = Codigo FROM inserted;
        SET @Detalle = CONCAT('Actualización de actividad. Cupo anterior: ', (SELECT Cupo FROM deleted), '. Cupo nuevo: ', (SELECT Cupo FROM inserted));
    END
    ELSE IF EXISTS (SELECT * FROM inserted)
    BEGIN
        SET @Accion = 'INSERT';
        SELECT @Codigo = Codigo FROM inserted;
        SET @Detalle = CONCAT('Nueva actividad creada: ', (SELECT Nombre FROM inserted));
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        SET @Accion = 'DELETE';
        SELECT @Codigo = Codigo FROM deleted;
        SET @Detalle = CONCAT('Actividad eliminada: ', (SELECT Nombre FROM deleted));
    END
    ELSE
        RETURN; 

    -- 2. INSERTAR EN LA TABLA DE AUDITORÍA
    -- Nota: El trigger no recibe parámetros de la app (como IdUsuarioAuditoria),
    -- así que registraremos 'SYSTEM' o ajustaremos según necesidad.
    INSERT INTO Auditoria_Actividades (CodigoActividad, Accion, IdUsuario, Detalle)
    VALUES (@Codigo, @Accion, 'AUTO-TRIGGER', @Detalle);
END;
GO

-- ==========================================================
-- ACTUALIZACION DE PROCEDURES (LIMPIOS)
-- ==========================================================
-- A) Insertar 
CREATE OR ALTER PROCEDURE sp_InsertarActividad
    @Codigo NVARCHAR(20),
    @Nombre NVARCHAR(100),
    @FechaRealizacion DATETIME,
    @Cupo INT,
    @IdResponsable NVARCHAR(15),
    @Latitud FLOAT,
    @Longitud FLOAT,
    @NombreEstado NVARCHAR(50),
    @NombreDiscapacidad NVARCHAR(100),
    @IdUsuarioAuditoria NVARCHAR(15) -- Se mantiene por compatibilidad con C#, aunque el trigger use otro.
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DECLARE @IdEstado INT = (SELECT TOP 1 IdEstado FROM Cat_Estados WHERE NombreEstado = @NombreEstado);
        DECLARE @IdDiscapacidad INT = (SELECT TOP 1 IdDiscapacidad FROM Cat_Discapacidades WHERE NombreDiscapacidad = @NombreDiscapacidad);
        IF @IdEstado IS NULL SET @IdEstado = 1; IF @IdDiscapacidad IS NULL SET @IdDiscapacidad = 5; 

        -- SOLO HACEMOS EL INSERT (El trigger salta automáticamente)
        INSERT INTO Actividades (Codigo, Nombre, FechaRealizacion, Cupo, IdResponsable, Latitud, Longitud, IdEstado, IdDiscapacidad)
        VALUES (@Codigo, @Nombre, @FechaRealizacion, @Cupo, @IdResponsable, @Latitud, @Longitud, @IdEstado, @IdDiscapacidad);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- B) Actualizar 
CREATE OR ALTER PROCEDURE sp_ActualizarActividad
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

        -- SOLO EL UPDATE (El trigger salta automático)
        UPDATE Actividades
        SET Nombre = @Nombre, FechaRealizacion = @FechaRealizacion, Cupo = @Cupo,
            IdResponsable = @IdResponsable,
            Latitud = @Latitud, Longitud = @Longitud,
            IdEstado = @IdEstado, IdDiscapacidad = @IdDiscapacidad  
        WHERE Codigo = @Codigo;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- C) Eliminar 
CREATE OR ALTER PROCEDURE sp_EliminarActividad
    @Codigo NVARCHAR(20),
    @IdUsuarioAuditoria NVARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- SOLO EL DELETE (El trigger salta automático)
        IF EXISTS (SELECT 1 FROM Actividades WHERE Codigo = @Codigo)
        BEGIN
            DELETE FROM Actividades WHERE Codigo = @Codigo;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO