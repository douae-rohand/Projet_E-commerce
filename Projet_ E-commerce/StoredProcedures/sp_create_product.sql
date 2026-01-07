IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_create_product')
    DROP PROCEDURE sp_create_product;
GO

CREATE PROCEDURE sp_create_product
    @idAdmin INT,
    @nomP NVARCHAR(255),
    @description NVARCHAR(MAX) = NULL,
    @seuil_alerte INT = 10,
    @idC INT,
    -- Première variante
    @prix DECIMAL(10,2),
    @taille NVARCHAR(20) = NULL,
    @couleur NVARCHAR(20) = NULL,
    @photo NVARCHAR(255) = NULL,
    @quantite INT,
    @poids NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Insérer le produit
        INSERT INTO Produits (nomP, description, seuil_alerte, statut, idC, idAdmin, created_at, updated_at)
        VALUES (@nomP, @description, @seuil_alerte, 'active', @idC, @idAdmin, GETDATE(), GETDATE());

        DECLARE @idP INT = SCOPE_IDENTITY();

        -- Insérer la première variante
        INSERT INTO Variantes (idP, prix, taille, couleur, photo, quantite, poids, created_at, updated_at)
        VALUES (@idP, @prix, @taille, @couleur, @photo, @quantite, @poids, GETDATE(), GETDATE());

        COMMIT TRANSACTION;

        -- Retourner l'ID du produit créé
        SELECT @idP AS idP, 'PRODUCT_CREATED' AS message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000);
        SET @ErrorMessage = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO
