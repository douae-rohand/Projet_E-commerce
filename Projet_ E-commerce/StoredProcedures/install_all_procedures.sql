-- ============================================
-- Script d'installation des procédures stockées
-- Projet E-commerce - Authentification
-- ============================================

USE [VotreBaseDeDonnees]; -- Remplacez par le nom de votre base de données
GO

-- ============================================
-- Procédure: sp_inscription_admin
-- Description: Inscription d'un administrateur de coopérative
-- ============================================
CREATE OR ALTER PROCEDURE sp_inscription_admin
    @email NVARCHAR(100),
    @password NVARCHAR(255),
    @nom_cooperative NVARCHAR(255),
    @localisation NVARCHAR(MAX),
    @ville NVARCHAR(255),
    @logo NVARCHAR(255),
    @telephone NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Vérifier si l'email existe déjà
        IF EXISTS (SELECT 1 FROM Utilisateurs WHERE email = @email)
        BEGIN
            RAISERROR('Email déjà utilisé', 16, 1);
            RETURN;
        END

        -- Insertion dans la table Utilisateurs
        INSERT INTO Utilisateurs (email, password, est_actif, created_at, updated_at)
        VALUES (@email, @password, 1, GETDATE(), GETDATE());

        DECLARE @userId INT = SCOPE_IDENTITY();

        -- Insertion dans la table Admins
        INSERT INTO Admins (
            id,
            nom_cooperative,
            localisation,
            ville,
            logo,
            telephone,
            created_at,
            updated_at
        )
        VALUES (
            @userId,
            @nom_cooperative,
            @localisation,
            @ville,
            @logo,
            @telephone,
            GETDATE(),
            GETDATE()
        );

        COMMIT TRANSACTION;

        -- Retourner un message de succès
        SELECT 'INSCRIPTION_ADMIN_OK' AS message, @userId AS id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO

-- ============================================
-- Procédure: sp_inscription_client
-- Description: Inscription d'un client
-- ============================================
CREATE OR ALTER PROCEDURE sp_inscription_client
    @email NVARCHAR(100),
    @password NVARCHAR(255),
    @prenom NVARCHAR(255),
    @nom NVARCHAR(255),
    @telephone NVARCHAR(100),
    @date_naissance DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Vérifier si l'email existe déjà
        IF EXISTS (SELECT 1 FROM Utilisateurs WHERE email = @email)
        BEGIN
            RAISERROR('Email déjà utilisé', 16, 1);
            RETURN;
        END

        -- Insertion dans la table Utilisateurs
        INSERT INTO Utilisateurs (email, password, est_actif, created_at, updated_at)
        VALUES (@email, @password, 1, GETDATE(), GETDATE());

        DECLARE @userId INT = SCOPE_IDENTITY();

        -- Insertion dans la table Clients
        INSERT INTO Clients (
            id,
            prenom,
            nom,
            telephone,
            date_naissance,
            created_at,
            updated_at
        )
        VALUES (
            @userId,
            @prenom,
            @nom,
            @telephone,
            @date_naissance,
            GETDATE(),
            GETDATE()
        );

        COMMIT TRANSACTION;

        -- Retourner un message de succès
        SELECT 'INSCRIPTION_CLIENT_OK' AS message, @userId AS id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO

-- ============================================
-- Procédure: sp_login_utilisateur
-- Description: Authentification d'un utilisateur
-- ============================================
CREATE OR ALTER PROCEDURE sp_login_utilisateur
    @email NVARCHAR(100),
    @password NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    -- Vérifier si l'utilisateur existe avec les bonnes credentials
    IF NOT EXISTS (
        SELECT 1 
        FROM Utilisateurs
        WHERE email = @email
          AND password = @password
          AND est_actif = 1
    )
    BEGIN
        RAISERROR('Email ou mot de passe incorrect', 16, 1);
        RETURN;
    END

    DECLARE @userId INT;

    -- Récupérer l'ID de l'utilisateur
    SELECT @userId = id
    FROM Utilisateurs
    WHERE email = @email;

    -- Déterminer le rôle et retourner les informations
    IF EXISTS (SELECT 1 FROM SuperAdmins WHERE id = @userId)
    BEGIN
        SELECT id, email, 'SUPER_ADMIN' AS role 
        FROM Utilisateurs 
        WHERE id = @userId;
    END
    ELSE IF EXISTS (SELECT 1 FROM Admins WHERE id = @userId)
    BEGIN
        SELECT id, email, 'ADMIN' AS role 
        FROM Utilisateurs 
        WHERE id = @userId;
    END
    ELSE IF EXISTS (SELECT 1 FROM Clients WHERE id = @userId)
    BEGIN
        SELECT id, email, 'CLIENT' AS role 
        FROM Utilisateurs 
        WHERE id = @userId;
    END
    ELSE
    BEGIN
        RAISERROR('Utilisateur sans rôle défini', 16, 1);
        RETURN;
    END
END;
GO

PRINT 'Toutes les procédures stockées ont été créées avec succès !';
