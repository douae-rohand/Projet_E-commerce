IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_inscription_client')
    DROP PROCEDURE sp_inscription_client;
GO

CREATE PROCEDURE sp_inscription_client
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

        DECLARE @userId INT;
        SET @userId = SCOPE_IDENTITY();

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
        
        DECLARE @ErrorMessage NVARCHAR(4000);
        SET @ErrorMessage = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
