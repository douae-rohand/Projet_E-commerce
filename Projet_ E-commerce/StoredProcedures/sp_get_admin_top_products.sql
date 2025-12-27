IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_get_admin_top_products')
    DROP PROCEDURE sp_get_admin_top_products;
GO

CREATE PROCEDURE sp_get_admin_top_products
    @idAdmin INT,
    @limit INT = 5
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@limit)
        p.idP AS IdP,
        p.nomP AS NomP,
        COUNT(lc.idLC) AS NombreVentes,
        SUM(lc.quantite * lc.prix_unitaire) AS RevenuTotal
    FROM Produits p
    INNER JOIN Variantes v ON p.idP = v.idP
    INNER JOIN LignesCommande lc ON v.idV = lc.idV
    INNER JOIN Commandes c ON lc.idCommande = c.idCommande
    WHERE p.idAdmin = @idAdmin
        AND c.statut != 'annule'
    GROUP BY p.idP, p.nomP
    ORDER BY NombreVentes DESC, RevenuTotal DESC;
END;
GO
