IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_get_order_items_admin')
    DROP PROCEDURE sp_get_order_items_admin;
GO

CREATE PROCEDURE sp_get_order_items_admin
    @idCommande INT,
    @idAdmin INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        lc.idLC AS IdLC,
        p.nomP AS NomProduit,
        v.taille AS Taille,
        v.couleur AS Couleur,
        lc.quantite AS Quantite,
        lc.prix_unitaire AS PrixUnitaire
        -- SousTotal calculated in C# DTO usually, effectively Quantite * PrixUnitaire
    FROM LignesCommande lc
    INNER JOIN Variantes v ON lc.idV = v.idV
    INNER JOIN Produits p ON v.idP = p.idP
    WHERE lc.idCommande = @idCommande
        AND p.idAdmin = @idAdmin;
END;
GO
