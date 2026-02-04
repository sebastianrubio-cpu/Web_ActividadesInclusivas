-- =============================================
-- 1. CONFIGURACIÓN DE LA BASE DE DATOS
-- =============================================
IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'SistemaInclusivoDB')
BEGIN
    CREATE DATABASE SistemaInclusivoDB;
END
GO

USE SistemaInclusivoDB;
GO

-- =============================================
-- 2. LIMPIEZA DE OBJETOS EXISTENTES
-- =============================================
IF OBJECT_ID('Auditoria_Actividades', 'U') IS NOT NULL DROP TABLE Auditoria_Actividades;
IF OBJECT_ID('Actividades', 'U') IS NOT NULL DROP TABLE Actividades;
IF OBJECT_ID('Usuarios', 'U') IS NOT NULL DROP TABLE Usuarios;
IF OBJECT_ID('Cat_Generos', 'U') IS NOT NULL DROP TABLE Cat_Generos;
IF OBJECT_ID('Cat_Discapacidades', 'U') IS NOT NULL DROP TABLE Cat_Discapacidades;
IF OBJECT_ID('Cat_Estados', 'U') IS NOT NULL DROP TABLE Cat_Estados;
GO

-- =============================================
-- 3. DEFINICIÓN DE LA ESTRUCTURA (TABLAS)
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

CREATE TABLE Usuarios (
    IdUsuario NVARCHAR(15) PRIMARY KEY, -- Cédula
    Correo NVARCHAR(100) NOT NULL UNIQUE,
    Clave NVARCHAR(100) NOT NULL, 
    Nombre NVARCHAR(100) NOT NULL,
    Rol NVARCHAR(50) NOT NULL CHECK (Rol IN ('Administrador', 'Profesor', 'Estudiante')),
    IdGenero INT NULL,
    CONSTRAINT FK_Usuarios_Generos FOREIGN KEY (IdGenero) REFERENCES Cat_Generos(IdGenero)
);

CREATE TABLE Actividades (
    Codigo NVARCHAR(20) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    FechaRealizacion DATETIME NOT NULL,
    Cupo INT NOT NULL CHECK (Cupo > 0),
    IdResponsable NVARCHAR(15) NOT NULL,
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
    Accion NVARCHAR(10),
    FechaAccion DATETIME DEFAULT GETDATE(),
    IdUsuario NVARCHAR(15) NULL,
    Detalle NVARCHAR(MAX),
    CONSTRAINT FK_Auditoria_Usuarios FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario)
);
GO

-- =============================================
-- 4. CARGA DE DATOS INICIALES (SEMILLA)
-- =============================================
INSERT INTO Cat_Estados (NombreEstado) VALUES ('Activo'), ('Inactivo'), ('Finalizado'), ('Lleno');
INSERT INTO Cat_Discapacidades (NombreDiscapacidad) VALUES ('Motriz'), ('Visual'), ('Auditiva'), ('Intelectual'), ('Ninguna');
INSERT INTO Cat_Generos (IdGenero, NombreGenero) VALUES (0, 'Masculino'), (1, 'Femenino'), (2, 'No Binario');

INSERT INTO Usuarios (IdUsuario, Correo, Clave, Nombre, Rol, IdGenero) 
VALUES ('1700000000', 'admin@uisek.edu.ec', '12345', 'Super Admin', 'Administrador', 0);

INSERT INTO Usuarios (IdUsuario, Correo, Clave, Nombre, Rol, IdGenero) 
VALUES ('1700000001', 'juan.perez@uisek.edu.ec', '12345', 'Juan Pérez', 'Profesor', 0);
GO

-- =============================================
-- 5. LÓGICA DE AUDITORÍA (TRIGGER)
-- =============================================
CREATE TRIGGER trg_Auditoria_Actividades
ON Actividades
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Accion NVARCHAR(10), @Detalle NVARCHAR(MAX), @Codigo NVARCHAR(20);

    IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
    BEGIN
        SET @Accion = 'UPDATE';
        SELECT @Codigo = Codigo FROM inserted;
        SET @Detalle = CONCAT('Actualización. Cupo anterior: ', (SELECT Cupo FROM deleted), '. Nuevo: ', (SELECT Cupo FROM inserted));
    END
    ELSE IF EXISTS (SELECT * FROM inserted)
    BEGIN
        SET @Accion = 'INSERT';
        SELECT @Codigo = Codigo FROM inserted;
        SET @Detalle = CONCAT('Creación de actividad: ', (SELECT Nombre FROM inserted));
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        SET @Accion = 'DELETE';
        SELECT @Codigo = Codigo FROM deleted;
        SET @Detalle = CONCAT('Eliminación de actividad: ', (SELECT Nombre FROM deleted));
    END

    INSERT INTO Auditoria_Actividades (CodigoActividad, Accion, IdUsuario, Detalle)
    VALUES (@Codigo, @Accion, 'SYSTEM_TRG', @Detalle);
END;
GO

-- =============================================
-- 6. PROCEDIMIENTOS ALMACENADOS (SPs)
-- =============================================

-- A) OBTENER TODAS
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
    @NombreEstado NVARCHAR(50), @NombreDiscapacidad NVARCHAR(100), @IdUsuarioAuditoria NVARCHAR(15)
AS
BEGIN
    BEGIN TRY
        DECLARE @IdE INT = (SELECT IdEstado FROM Cat_Estados WHERE NombreEstado = @NombreEstado);
        DECLARE @IdD INT = (SELECT IdDiscapacidad FROM Cat_Discapacidades WHERE NombreDiscapacidad = @NombreDiscapacidad);
        INSERT INTO Actividades (Codigo, Nombre, FechaRealizacion, Cupo, IdResponsable, Latitud, Longitud, IdEstado, IdDiscapacidad)
        VALUES (@Codigo, @Nombre, @FechaRealizacion, @Cupo, @IdResponsable, @Latitud, @Longitud, ISNULL(@IdE,1), ISNULL(@IdD,5));
    END TRY
    BEGIN CATCH THROW; END CATCH
END;
GO

-- D) ACTUALIZAR
CREATE PROCEDURE sp_ActualizarActividad
    @Codigo NVARCHAR(20), @Nombre NVARCHAR(100), @FechaRealizacion DATETIME, @Cupo INT,
    @IdResponsable NVARCHAR(15), @Latitud FLOAT, @Longitud FLOAT, 
    @NombreEstado NVARCHAR(50), @NombreDiscapacidad NVARCHAR(100), @IdUsuarioAuditoria NVARCHAR(15)
AS
BEGIN
    BEGIN TRY
        DECLARE @IdE INT = (SELECT IdEstado FROM Cat_Estados WHERE NombreEstado = @NombreEstado);
        DECLARE @IdD INT = (SELECT IdDiscapacidad FROM Cat_Discapacidades WHERE NombreDiscapacidad = @NombreDiscapacidad);
        UPDATE Actividades SET Nombre=@Nombre, FechaRealizacion=@FechaRealizacion, Cupo=@Cupo, IdResponsable=@IdResponsable,
                               Latitud=@Latitud, Longitud=@Longitud, IdEstado=ISNULL(@IdE,1), IdDiscapacidad=ISNULL(@IdD,5)
        WHERE Codigo = @Codigo;
    END TRY
    BEGIN CATCH THROW; END CATCH
END;
GO

-- E) ELIMINAR
CREATE PROCEDURE sp_EliminarActividad @Codigo NVARCHAR(20), @IdUsuarioAuditoria NVARCHAR(15) AS
BEGIN
    DELETE FROM Actividades WHERE Codigo = @Codigo;
END;
GO

/* ESTE QUERY DEFINE LA ESTRUCTURA COMPLETA DEL SISTEMA INCLUSIVO UISEK. 
INCLUYE LA NORMALIZACIÓN EN TERCERA FORMA NORMAL MEDIANTE CATÁLOGOS, 
UN MECANISMO DE AUDITORÍA AUTOMATIZADO POR TRIGGERS PARA TRAZABILIDAD 
Y UNA CAPA DE PROCEDIMIENTOS ALMACENADOS QUE ENCAPSULA LA LÓGICA DE NEGOCIO 
Y PROTEGE LA INTEGRIDAD TRANSACCIONAL DE LOS DATOS.
*/