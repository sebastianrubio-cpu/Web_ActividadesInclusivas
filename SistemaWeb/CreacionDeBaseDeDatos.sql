
CREATE DATABASE SistemaInclusivoDB;
GO
USE SistemaInclusivoDB;
GO

-- 1. TABLAS CATÁLOGO
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

-- Datos Semilla
INSERT INTO Cat_Estados VALUES ('Activo'), ('Inactivo'), ('Finalizado');
INSERT INTO Cat_Discapacidades VALUES ('Motriz'), ('Visual'), ('Auditiva'), ('Intelectual'), ('Ninguna');
INSERT INTO Cat_Generos VALUES (0, 'Masculino'), (1, 'Femenino'), (2, 'No Binario');
GO

-- 2. TABLA USUARIOS (Aquí están los datos reales del Responsable)
CREATE TABLE Usuarios (
    IdUsuario NVARCHAR(15) PRIMARY KEY, -- Cédula (PK)
    Correo NVARCHAR(100) NOT NULL UNIQUE, -- GmailProfesor sale de aquí
    Clave NVARCHAR(100) NOT NULL, 
    Nombre NVARCHAR(100) NOT NULL, -- Responsable (Nombre) sale de aquí
    Rol NVARCHAR(50) NOT NULL CHECK (Rol IN ('Administrador', 'Profesor', 'Estudiante')),
    IdGenero INT NULL,
    CONSTRAINT FK_Usuarios_Generos FOREIGN KEY (IdGenero) REFERENCES Cat_Generos(IdGenero)
);

-- Insertamos un Admin y un Profesor para probar
INSERT INTO Usuarios VALUES ('1700000000', 'admin@uisek.edu.ec', '12345', 'Super Admin', 'Administrador', 0);
INSERT INTO Usuarios VALUES ('1700000001', 'juan.perez@uisek.edu.ec', '12345', 'Juan Pérez', 'Profesor', 0);
GO

-- 3. TABLA ACTIVIDADES (NORMALIZADA)
CREATE TABLE Actividades (
    Codigo NVARCHAR(20) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    FechaRealizacion DATETIME NOT NULL,
    Cupo INT NOT NULL CHECK (Cupo > 0),
    
    -- CAMBIO: FK hacia Usuarios.
    -- Aquí SOLO guardamos la Cédula (ej: '1700000001').
    -- NO guardamos ni nombre ni correo.
    IdResponsable NVARCHAR(15) NOT NULL,

    Latitud FLOAT NOT NULL DEFAULT -0.09106038310321836,
    Longitud FLOAT NOT NULL DEFAULT -78.4838308629782,
    IdEstado INT NOT NULL,
    IdDiscapacidad INT NOT NULL,

    -- RELACIÓN CLAVE (FK):
    CONSTRAINT FK_Actividades_Usuarios FOREIGN KEY (IdResponsable) REFERENCES Usuarios(IdUsuario),
    
    CONSTRAINT FK_Actividades_Estados FOREIGN KEY (IdEstado) REFERENCES Cat_Estados(IdEstado),
    CONSTRAINT FK_Actividades_Discapacidad FOREIGN KEY (IdDiscapacidad) REFERENCES Cat_Discapacidades(IdDiscapacidad)
);
GO

-- 4. AUDITORÍA (Independiente / Backlog)
CREATE TABLE Auditoria_Actividades (
    IdAuditoria INT IDENTITY(1,1) PRIMARY KEY,
    CodigoActividad NVARCHAR(20), -- Sin FK para que funcione como histórico si borran la actividad
    Accion NVARCHAR(10),
    FechaAccion DATETIME DEFAULT GETDATE(),
    IdUsuario NVARCHAR(15) NULL, -- Quién hizo la acción (FK a Usuarios)
    Detalle NVARCHAR(MAX),

    CONSTRAINT FK_Auditoria_Usuarios FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario)
);
GO

-- 5. STORED PROCEDURES (Adaptados a la nueva estructura)

-- A) SP OBTENER (IMPORTANTE: Aquí se reconstruyen los datos)
CREATE OR ALTER PROCEDURE sp_ObtenerActividades AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        A.Codigo, 
        A.Nombre, 
        A.FechaRealizacion, 
        A.Cupo, 
        
        -- AQUÍ OCURRE LA MAGIA:
        -- Aunque la tabla Actividades solo tiene el ID,
        -- el JOIN nos trae el Nombre y el Correo de la tabla Usuarios.
        U.Nombre AS Responsable, 
        U.Correo AS GmailProfesor,
        
        A.Latitud, 
        A.Longitud,
        E.NombreEstado AS Estado, 
        D.NombreDiscapacidad AS TipoDiscapacidad 
    FROM Actividades A
    INNER JOIN Usuarios U ON A.IdResponsable = U.IdUsuario -- <--- LA UNIÓN
    INNER JOIN Cat_Estados E ON A.IdEstado = E.IdEstado
    INNER JOIN Cat_Discapacidades D ON A.IdDiscapacidad = D.IdDiscapacidad;
END;
GO

-- B) SP OBTENER POR ID
CREATE OR ALTER PROCEDURE sp_ObtenerActividadPorId @Codigo NVARCHAR(20) AS
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

-- C) SP INSERTAR (Pide el ID del Responsable, no el nombre)
CREATE OR ALTER PROCEDURE sp_InsertarActividad
    @Codigo NVARCHAR(20),
    @Nombre NVARCHAR(100),
    @FechaRealizacion DATETIME,
    @Cupo INT,
    @IdResponsable NVARCHAR(15), -- <--- Cédula del profesor
    @Latitud FLOAT,
    @Longitud FLOAT,
    @NombreEstado NVARCHAR(50),
    @NombreDiscapacidad NVARCHAR(100),
    @IdUsuarioAuditoria NVARCHAR(15) -- Quién está ejecutando (Auditoría)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DECLARE @IdEstado INT = (SELECT TOP 1 IdEstado FROM Cat_Estados WHERE NombreEstado = @NombreEstado);
        DECLARE @IdDiscapacidad INT = (SELECT TOP 1 IdDiscapacidad FROM Cat_Discapacidades WHERE NombreDiscapacidad = @NombreDiscapacidad);
        IF @IdEstado IS NULL SET @IdEstado = 1; IF @IdDiscapacidad IS NULL SET @IdDiscapacidad = 5; 

        -- Insertamos usando el ID
        INSERT INTO Actividades (Codigo, Nombre, FechaRealizacion, Cupo, IdResponsable, Latitud, Longitud, IdEstado, IdDiscapacidad)
        VALUES (@Codigo, @Nombre, @FechaRealizacion, @Cupo, @IdResponsable, @Latitud, @Longitud, @IdEstado, @IdDiscapacidad);

        -- Para el log, podemos buscar el nombre si queremos que quede bonito
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

-- D) SP ACTUALIZAR
CREATE OR ALTER PROCEDURE sp_ActualizarActividad
    @Codigo NVARCHAR(20),
    @Nombre NVARCHAR(100),
    @FechaRealizacion DATETIME,
    @Cupo INT,
    @IdResposable NVARCHAR(15), -- <--- Cédula del profesor
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
            IdResponsable = @IdResponsable, -- Actualizamos la FK
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

-- E) SP ELIMINAR (Mantiene backlog fantasma)
CREATE OR ALTER PROCEDURE sp_EliminarActividad
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
            
            -- Borramos la actividad (y sus referencias FK se validan)
            DELETE FROM Actividades WHERE Codigo = @Codigo;

            -- Insertamos en log (CodigoActividad es texto, así que no falla)
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