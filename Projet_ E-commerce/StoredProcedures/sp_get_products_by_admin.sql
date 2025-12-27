IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_get_products_by_admin')
    DROP PROCEDURE sp_get_products_by_admin;
GO

CREATE PROCEDURE sp_get_products_by_admin
    @idAdmin INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        p.idP AS IdP,
        p.nomP AS NomP,
        p.description AS Description,
        p.seuil_alerte AS SeuilAlerte,
        p.statut AS Statut,
        p.idC AS IdC,
        c.nom AS NomCategorie,
        p.created_at AS CreatedAt,
        p.updated_at AS UpdatedAt,
        -- Variantes (JSON-like concatenation)
        (SELECT COUNT(*) FROM Variantes WHERE idP = p.idP) AS NombreVariantes,
        -- Stock total
        ISNULL((SELECT SUM(quantite) FROM Variantes WHERE idP = p.idP), 0) AS StockTotal,
        -- Prix minimum et maximum
        (SELECT MIN(prix) FROM Variantes WHERE idP = p.idP) AS PrixMin,
        (SELECT MAX(prix) FROM Variantes WHERE idP = p.idP) AS PrixMax
    FROM Produits p
    INNER JOIN Categories c ON p.idC = c.idC
    WHERE p.idAdmin = @idAdmin
    ORDER BY p.created_at DESC;
END;
GO
