-- 1. CREACIÓN DE LA BASE DE DATOS
CREATE DATABASE SistemaInclusivoDB;
GO
USE SistemaInclusivoDB;
GO

-- 2. TABLAS CATÁLOGO

-- Estados de Actividades
CREATE TABLE Cat_Estados (
    IdEstado INT IDENTITY(1,1) PRIMARY KEY,
    NombreEstado NVARCHAR(50) NOT NULL UNIQUE
);

-- Discapacidades
CREATE TABLE Cat_Discapacidades (
    IdDiscapacidad INT IDENTITY(1,1) PRIMARY KEY,
    NombreDiscapacidad NVARCHAR(100) NOT NULL UNIQUE
);

-- Géneros (NUEVA TABLA)
CREATE TABLE Cat_Generos (
    IdGenero INT PRIMARY KEY,
    NombreGenero NVARCHAR(20) NOT NULL
);

-- Seed Data (Datos Iniciales)
INSERT INTO Cat_Estados (NombreEstado) VALUES ('Activo'), ('Inactivo'), ('Finalizado');
INSERT INTO Cat_Discapacidades (NombreDiscapacidad) VALUES ('Motriz'), ('Visual'), ('Auditiva'), ('Intelectual'), ('Ninguna');
INSERT INTO Cat_Generos (IdGenero, NombreGenero) VALUES (0, 'Masculino'), (1, 'Femenino'), (2, 'No Binario');
GO

-- 3. TABLA USUARIOS (MODIFICADA: IdUsuario es la Cédula)
CREATE TABLE Usuarios (
    IdUsuario NVARCHAR(15) PRIMARY KEY, -- Aquí se almacena la Cédula
    Correo NVARCHAR(100) NOT NULL UNIQUE,
    Clave NVARCHAR(100) NOT NULL, 
    Nombre NVARCHAR(100) NOT NULL,
    Rol NVARCHAR(50) NOT NULL CHECK (Rol IN ('Administrador', 'Profesor', 'Estudiante')),
    IdGenero INT NULL,
    
    CONSTRAINT FK_Usuarios_Generos FOREIGN KEY (IdGenero) REFERENCES Cat_Generos(IdGenero)
);

-- Insertar Administrador (Cédula ficticia '1700000000', Género 0)
INSERT INTO Usuarios (IdUsuario, Correo, Clave, Nombre, Rol, IdGenero) 
VALUES ('1700000000', 'sebastian.rubio@uisek.edu.ec', '12345', 'Administrador Sistema', 'Administrador', 0);
GO

-- 4. (TABLA ESTUDIANTES ELIMINADA) --

-- 5. TABLA ACTIVIDADES 
CREATE TABLE Actividades (
    Codigo NVARCHAR(20) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    FechaRealizacion DATETIME NOT NULL,
    Cupo INT NOT NULL CHECK (Cupo > 0),
    Responsable NVARCHAR(100) NOT NULL DEFAULT 'Sin Asignar',
    GmailProfesor NVARCHAR(100) NOT NULL DEFAULT 'contacto@uisek.edu.ec',
    
    -- COORDENADAS PARA MAPA (Con Default UISEK)
    Latitud FLOAT NOT NULL DEFAULT -0.09106038310321836,
    Longitud FLOAT NOT NULL DEFAULT -78.4838308629782,

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
            WHEN @Accion = 'UPDATE' THEN CONCAT('Cambio IdEstado: ', d.IdEstado, ' -> ', i.IdEstado, '. Coords: ', i.Latitud, ',', i.Longitud)
            WHEN @Accion = 'INSERT' THEN CONCAT('Nueva actividad: ', i.Nombre)
            WHEN @Accion = 'DELETE' THEN CONCAT('Eliminada: ', d.Nombre)
        END
    FROM inserted i
    FULL OUTER JOIN deleted d ON i.Codigo = d.Codigo;
END;
GO

-- 7. STORED PROCEDURES

-- SP: Obtener Todo
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
        A.Latitud,
        A.Longitud,
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
        A.Latitud,
        A.Longitud,
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
    @Latitud FLOAT,
    @Longitud FLOAT,
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

        INSERT INTO Actividades (Codigo, Nombre, FechaRealizacion, Cupo, Responsable, GmailProfesor, Latitud, Longitud, IdEstado, IdDiscapacidad)
        VALUES (@Codigo, @Nombre, @FechaRealizacion, @Cupo, @Responsable, @GmailProfesor, @Latitud, @Longitud, @IdEstado, @IdDiscapacidad);

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
    @Latitud FLOAT,
    @Longitud FLOAT,
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
            Latitud = @Latitud,
            Longitud = @Longitud,
            IdEstado = @IdEstado,             
            IdDiscapacidad = @IdDiscapacidad  
        WHERE Codigo = @Codigo;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SP: Eliminar
CREATE PROCEDURE sp_EliminarActividad
    @Codigo NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
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