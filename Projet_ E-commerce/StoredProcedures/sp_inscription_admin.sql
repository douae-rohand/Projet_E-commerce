IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_inscription_admin')
    DROP PROCEDURE sp_inscription_admin;
GO

CREATE PROCEDURE sp_inscription_admin
    @email NVARCHAR(100),
    @password NVARCHAR(255),
    @nom_cooperative NVARCHAR(255),
    @localisation NVARCHAR(MAX),
    @ville NVARCHAR(255),
    @logo NVARCHAR(255),
    @telephone NVARCHAR(100),
    @description NVARCHAR(MAX) = NULL
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

        DECLARE @userId INT;
        SET @userId = SCOPE_IDENTITY();

        -- Insertion dans la table Admins
        INSERT INTO Admins (
            id,
            nom_cooperative,
            localisation,
            ville,
            logo,
            telephone,
            description,
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
            @description,
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
        
        DECLARE @ErrorMessage NVARCHAR(4000);
        SET @ErrorMessage = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
