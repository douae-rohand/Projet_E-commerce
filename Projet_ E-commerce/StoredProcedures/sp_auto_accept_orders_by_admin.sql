USE [cooperative]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_auto_accept_orders_by_admin')
    DROP PROCEDURE sp_auto_accept_orders_by_admin;
GO

CREATE PROCEDURE sp_auto_accept_orders_by_admin
    @idAdmin INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

        BEGIN TRANSACTION;

        -- Utiliser une variable table pour stocker les candidats temporairement
        DECLARE @Candidates TABLE (idCommande INT PRIMARY KEY);

        INSERT INTO @Candidates (idCommande)
        SELECT 
            c.idCommande
        FROM [dbo].[Commandes] c
        INNER JOIN [dbo].[LignesCommande] lc ON c.idCommande = lc.idCommande
        INNER JOIN [dbo].[Variantes] v ON lc.idV = v.idV
        INNER JOIN [dbo].[Produits] p ON v.idP = p.idP
        WHERE c.statut = 'en_attente'
            AND p.idAdmin = @idAdmin
        GROUP BY c.idCommande
        HAVING SUM(CASE WHEN lc.quantite > v.quantite THEN 1 ELSE 0 END) = 0;

        -- Décrémenter le stock des variantes concernées
        UPDATE v
        SET v.quantite = v.quantite - lc.quantite,
            v.updated_at = GETDATE()
        FROM [dbo].[Variantes] v
        INNER JOIN [dbo].[LignesCommande] lc ON v.idV = lc.idV
        INNER JOIN [dbo].[Produits] p ON v.idP = p.idP
        INNER JOIN @Candidates c ON c.idCommande = lc.idCommande
        WHERE p.idAdmin = @idAdmin;

        -- Mettre à jour les commandes
        UPDATE [dbo].[Commandes]
        SET statut = 'en_preparation',
            updated_at = GETDATE()
        WHERE idCommande IN (SELECT idCommande FROM @Candidates);

        SELECT COUNT(*) AS AcceptedCount FROM @Candidates;

        COMMIT TRANSACTION;
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
