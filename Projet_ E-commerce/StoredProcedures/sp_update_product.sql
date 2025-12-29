IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_update_product')
    DROP PROCEDURE sp_update_product;
GO

CREATE PROCEDURE sp_update_product
    @idP INT,
    @idAdmin INT,
    @nomP NVARCHAR(255),
    @description NVARCHAR(MAX) = NULL,
    @seuil_alerte INT,
    @idC INT,
    @statut NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Vérifier que le produit appartient à l'admin
        IF NOT EXISTS (SELECT 1 FROM Produits WHERE idP = @idP AND idAdmin = @idAdmin)
        BEGIN
            RAISERROR('Produit non trouvé ou accès non autorisé', 16, 1);
            RETURN;
        END

        -- Mettre à jour le produit
        UPDATE Produits
        SET 
            nomP = @nomP,
            description = @description,
            seuil_alerte = @seuil_alerte,
            idC = @idC,
            statut = @statut,
            updated_at = GETDATE()
        WHERE idP = @idP AND idAdmin = @idAdmin;

        SELECT 'PRODUCT_UPDATED' AS message;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000);
        SET @ErrorMessage = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO
