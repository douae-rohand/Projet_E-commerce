IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_delete_product')
    DROP PROCEDURE sp_delete_product;
GO

CREATE PROCEDURE sp_delete_product
    @idP INT,
    @idAdmin INT
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

        -- Soft delete: changer le statut à 'inactive'
        UPDATE Produits
        SET 
            statut = 'inactive',
            updated_at = GETDATE()
        WHERE idP = @idP AND idAdmin = @idAdmin;

        SELECT 'PRODUCT_DELETED' AS message;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000);
        SET @ErrorMessage = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO
