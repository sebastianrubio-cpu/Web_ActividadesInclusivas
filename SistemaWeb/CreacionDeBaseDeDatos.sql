-- 1. CREACIÓN DE LA BASE DE DATOS
CREATE DATABASE SistemaInclusivoDB;
GO
USE SistemaInclusivoDB;
GO

-- 2. TABLAS CATÁLOGO (Normalización)
CREATE TABLE Cat_Estados (
    IdEstado INT IDENTITY(1,1) PRIMARY KEY,
    NombreEstado NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Cat_Discapacidades (
    IdDiscapacidad INT IDENTITY(1,1) PRIMARY KEY,
    NombreDiscapacidad NVARCHAR(100) NOT NULL UNIQUE
);

-- Seed Data (Datos obligatorios)
INSERT INTO Cat_Estados (NombreEstado) VALUES ('Activo'), ('Inactivo'), ('Finalizado');
INSERT INTO Cat_Discapacidades (NombreDiscapacidad) VALUES ('Motriz'), ('Visual'), ('Auditiva'), ('Intelectual'), ('Ninguna');
GO

-- 3. TABLA USUARIOS 
CREATE TABLE Usuarios (
    IdUsuario INT IDENTITY(1,1) PRIMARY KEY,
    Correo NVARCHAR(100) NOT NULL UNIQUE,
    Clave NVARCHAR(100) NOT NULL, 
    Nombre NVARCHAR(100) NOT NULL,
    Rol NVARCHAR(50) NOT NULL CHECK (Rol IN ('Administrador', 'Profesor', 'Estudiante'))
);

-- Usuario Admin por defecto
INSERT INTO Usuarios (Correo, Clave, Nombre, Rol) 
VALUES ('sebastian.rubio@uisek.edu.ec', '12345', 'Administrador Sistema', 'Administrador');
GO

-- 4. TABLA ESTUDIANTES 
CREATE TABLE Estudiantes (
    IdEstudiante INT IDENTITY(1,1) PRIMARY KEY,
    Cedula NVARCHAR(10) NOT NULL UNIQUE,
    Nombre NVARCHAR(100) NOT NULL,
    Apellido NVARCHAR(100) NOT NULL,
    Carrera NVARCHAR(100),
    Semestre INT
);
GO

-- 5. TABLA ACTIVIDADES 
CREATE TABLE Actividades (
    Codigo NVARCHAR(20) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    FechaRealizacion DATETIME NOT NULL,
    Cupo INT NOT NULL CHECK (Cupo > 0),
    Responsable NVARCHAR(100) NOT NULL DEFAULT 'Sin Asignar',
    GmailProfesor NVARCHAR(100) NOT NULL DEFAULT 'contacto@uisek.edu.ec',
    
    -- Foreign Keys 
    IdEstado INT NOT NULL,
    IdDiscapacidad INT NOT NULL,

    CONSTRAINT FK_Actividades_Estados FOREIGN KEY (IdEstado) REFERENCES Cat_Estados(IdEstado),
    CONSTRAINT FK_Actividades_Discapacidad FOREIGN KEY (IdDiscapacidad) REFERENCES Cat_Discapacidades(IdDiscapacidad)
);
GO

-- 6. AUDITORÍA Y TRIGGERS
CREATE TABLE Auditoria_Actividades (
    IdAuditoria INT IDENTITY(1,1) PRIMARY KEY,
    CodigoActividad NVARCHAR(20),
    Accion NVARCHAR(10),
    FechaAccion DATETIME DEFAULT GETDATE(),
    UsuarioBD NVARCHAR(100) DEFAULT SYSTEM_USER,
    Detalle NVARCHAR(MAX)
);
GO

CREATE TRIGGER trg_Auditoria_Actividades
ON Actividades
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Accion NVARCHAR(10);
    
    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        IF EXISTS (SELECT * FROM deleted)
            SET @Accion = 'UPDATE';
        ELSE
            SET @Accion = 'INSERT';
    END
    ELSE
        SET @Accion = 'DELETE';

    INSERT INTO Auditoria_Actividades (CodigoActividad, Accion, Detalle)
    SELECT 
        COALESCE(i.Codigo, d.Codigo),
        @Accion,
        CASE 
            WHEN @Accion = 'UPDATE' THEN CONCAT('Cambio IdEstado: ', d.IdEstado, ' -> ', i.IdEstado, '. Cupo: ', d.Cupo, ' -> ', i.Cupo)
            WHEN @Accion = 'INSERT' THEN CONCAT('Nueva actividad: ', i.Nombre)
            WHEN @Accion = 'DELETE' THEN CONCAT('Eliminada: ', d.Nombre)
        END
    FROM inserted i
    FULL OUTER JOIN deleted d ON i.Codigo = d.Codigo;
END;
GO

-- 7. STORED PROCEDURES

-- SP: Obtener 
CREATE OR ALTER PROCEDURE sp_ObtenerActividades
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        A.Codigo,
        A.Nombre,
        A.FechaRealizacion,
        A.Cupo,
        A.Responsable,
        A.GmailProfesor,
        E.NombreEstado AS Estado,             
        D.NombreDiscapacidad AS TipoDiscapacidad 
    FROM Actividades A
    INNER JOIN Cat_Estados E ON A.IdEstado = E.IdEstado
    INNER JOIN Cat_Discapacidades D ON A.IdDiscapacidad = D.IdDiscapacidad;
END;
GO

-- SP: Obtener por ID
CREATE OR ALTER PROCEDURE sp_ObtenerActividadPorId
    @Codigo NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        A.Codigo,
        A.Nombre,
        A.FechaRealizacion,
        A.Cupo,
        A.Responsable,
        A.GmailProfesor,
        E.NombreEstado AS Estado,
        D.NombreDiscapacidad AS TipoDiscapacidad
    FROM Actividades A
    INNER JOIN Cat_Estados E ON A.IdEstado = E.IdEstado
    INNER JOIN Cat_Discapacidades D ON A.IdDiscapacidad = D.IdDiscapacidad
    WHERE A.Codigo = @Codigo;
END;
GO

-- SP: Insertar 
CREATE OR ALTER PROCEDURE sp_InsertarActividad
    @Codigo NVARCHAR(20),
    @Nombre NVARCHAR(100),
    @FechaRealizacion DATETIME,
    @Cupo INT,
    @Responsable NVARCHAR(100),
    @GmailProfesor NVARCHAR(100),
    @NombreEstado NVARCHAR(50),
    @NombreDiscapacidad NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @IdEstado INT = (SELECT TOP 1 IdEstado FROM Cat_Estados WHERE NombreEstado = @NombreEstado);
        DECLARE @IdDiscapacidad INT = (SELECT TOP 1 IdDiscapacidad FROM Cat_Discapacidades WHERE NombreDiscapacidad = @NombreDiscapacidad);

        -- Defaults defensivos
        IF @IdEstado IS NULL SET @IdEstado = 1; 
        IF @IdDiscapacidad IS NULL SET @IdDiscapacidad = 5; 

        INSERT INTO Actividades (Codigo, Nombre, FechaRealizacion, Cupo, Responsable, GmailProfesor, IdEstado, IdDiscapacidad)
        VALUES (@Codigo, @Nombre, @FechaRealizacion, @Cupo, @Responsable, @GmailProfesor, @IdEstado, @IdDiscapacidad);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SP: Actualizar
CREATE OR ALTER PROCEDURE sp_ActualizarActividad
    @Codigo NVARCHAR(20),
    @Nombre NVARCHAR(100),
    @FechaRealizacion DATETIME,
    @Cupo INT,
    @Responsable NVARCHAR(100),
    @GmailProfesor NVARCHAR(100),
    @NombreEstado NVARCHAR(50),
    @NombreDiscapacidad NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @IdEstado INT = (SELECT TOP 1 IdEstado FROM Cat_Estados WHERE NombreEstado = @NombreEstado);
        DECLARE @IdDiscapacidad INT = (SELECT TOP 1 IdDiscapacidad FROM Cat_Discapacidades WHERE NombreDiscapacidad = @NombreDiscapacidad);
        
        IF @IdEstado IS NULL SET @IdEstado = 1;
        IF @IdDiscapacidad IS NULL SET @IdDiscapacidad = 5;

        UPDATE Actividades
        SET 
            Nombre = @Nombre,
            FechaRealizacion = @FechaRealizacion,
            Cupo = @Cupo,
            Responsable = @Responsable,
            GmailProfesor = @GmailProfesor,
            IdEstado = @IdEstado,             -- Corregido nombre columna
            IdDiscapacidad = @IdDiscapacidad  -- Corregido nombre columna
        WHERE Codigo = @Codigo;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- 5. SP: Eliminar 
DROP PROCEDURE IF EXISTS sp_EliminarActividad;
GO
CREATE PROCEDURE sp_EliminarActividad
    @Codigo NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DELETE FROM Actividades WHERE Codigo = @Codigo;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
GO