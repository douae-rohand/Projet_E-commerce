IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_accept_order')
    DROP PROCEDURE sp_accept_order;
GO

CREATE PROCEDURE sp_accept_order
    @idCommande INT,
    @idAdmin INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Vérifier que la commande contient des produits de cet admin
        IF NOT EXISTS (
            SELECT 1 
            FROM Commandes c
            INNER JOIN LignesCommande lc ON c.idCommande = lc.idCommande
            INNER JOIN Variantes v ON lc.idV = v.idV
            INNER JOIN Produits p ON v.idP = p.idP
            WHERE c.idCommande = @idCommande AND p.idAdmin = @idAdmin
        )
        BEGIN
            RAISERROR('Commande non trouvée ou accès non autorisé', 16, 1);
            RETURN;
        END

        -- Vérifier que la commande est en attente
        IF NOT EXISTS (SELECT 1 FROM Commandes WHERE idCommande = @idCommande AND statut = 'en_attente')
        BEGIN
            RAISERROR('La commande n''est pas en attente de validation', 16, 1);
            RETURN;
        END

        -- Mettre à jour le statut de la commande
        UPDATE Commandes
        SET 
            statut = 'valide',
            updated_at = GETDATE()
        WHERE idCommande = @idCommande;

        SELECT 'ORDER_ACCEPTED' AS message;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000);
        SET @ErrorMessage = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO
