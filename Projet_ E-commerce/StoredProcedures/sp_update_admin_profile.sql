IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_update_admin_profile')
    DROP PROCEDURE sp_update_admin_profile;
GO

CREATE PROCEDURE sp_update_admin_profile
    @idAdmin INT,
    @nom_cooperative NVARCHAR(255),
    @localisation NVARCHAR(MAX) = NULL,
    @ville NVARCHAR(255) = NULL,
    @telephone NVARCHAR(100) = NULL,
    @description NVARCHAR(MAX) = NULL,
    @logo NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Vérifier que l'admin existe
        IF NOT EXISTS (SELECT 1 FROM Admins WHERE id = @idAdmin)
        BEGIN
            RAISERROR('Admin non trouvé', 16, 1);
            RETURN;
        END

        -- Mettre à jour le profil admin
        UPDATE Admins
        SET 
            nom_cooperative = @nom_cooperative,
            localisation = @localisation,
            ville = @ville,
            telephone = @telephone,
            description = @description,
            logo = ISNULL(@logo, logo),  -- Ne mettre à jour que si un nouveau logo est fourni
            updated_at = GETDATE()
        WHERE id = @idAdmin;

        SELECT 'PROFILE_UPDATED' AS message;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000);
        SET @ErrorMessage = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO
