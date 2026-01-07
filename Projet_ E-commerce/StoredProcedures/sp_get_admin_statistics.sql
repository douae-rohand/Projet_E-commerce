IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_get_admin_statistics')
    DROP PROCEDURE sp_get_admin_statistics;
GO

CREATE PROCEDURE sp_get_admin_statistics
    @idAdmin INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurrentMonth INT = MONTH(GETDATE());
    DECLARE @CurrentYear INT = YEAR(GETDATE());
    DECLARE @LastMonth INT = MONTH(DATEADD(MONTH, -1, GETDATE()));
    DECLARE @LastMonthYear INT = YEAR(DATEADD(MONTH, -1, GETDATE()));

    -- Ventes du mois actuel
    DECLARE @CurrentMonthSales DECIMAL(10,2);
    SELECT @CurrentMonthSales = ISNULL(SUM(c.prixTotal), 0)
    FROM Commandes c
    INNER JOIN LignesCommande lc ON c.idCommande = lc.idCommande
    INNER JOIN Variantes v ON lc.idV = v.idV
    INNER JOIN Produits p ON v.idP = p.idP
    WHERE p.idAdmin = @idAdmin
        AND c.statut != 'annule'
        AND MONTH(c.created_at) = @CurrentMonth
        AND YEAR(c.created_at) = @CurrentYear;

    -- Ventes du mois dernier
    DECLARE @LastMonthSales DECIMAL(10,2);
    SELECT @LastMonthSales = ISNULL(SUM(c.prixTotal), 0)
    FROM Commandes c
    INNER JOIN LignesCommande lc ON c.idCommande = lc.idCommande
    INNER JOIN Variantes v ON lc.idV = v.idV
    INNER JOIN Produits p ON v.idP = p.idP
    WHERE p.idAdmin = @idAdmin
        AND c.statut != 'annule'
        AND MONTH(c.created_at) = @LastMonth
        AND YEAR(c.created_at) = @LastMonthYear;

    -- Nombre de commandes du mois actuel
    DECLARE @CurrentMonthOrders INT;
    SELECT @CurrentMonthOrders = COUNT(DISTINCT c.idCommande)
    FROM Commandes c
    INNER JOIN LignesCommande lc ON c.idCommande = lc.idCommande
    INNER JOIN Variantes v ON lc.idV = v.idV
    INNER JOIN Produits p ON v.idP = p.idP
    WHERE p.idAdmin = @idAdmin
        AND c.statut != 'annule'
        AND MONTH(c.created_at) = @CurrentMonth
        AND YEAR(c.created_at) = @CurrentYear;

    -- Nombre de commandes du mois dernier
    DECLARE @LastMonthOrders INT;
    SELECT @LastMonthOrders = COUNT(DISTINCT c.idCommande)
    FROM Commandes c
    INNER JOIN LignesCommande lc ON c.idCommande = lc.idCommande
    INNER JOIN Variantes v ON lc.idV = v.idV
    INNER JOIN Produits p ON v.idP = p.idP
    WHERE p.idAdmin = @idAdmin
        AND c.statut != 'annule'
        AND MONTH(c.created_at) = @LastMonth
        AND YEAR(c.created_at) = @LastMonthYear;

    -- Nombre de produits actifs
    DECLARE @ActiveProducts INT;
    SELECT @ActiveProducts = COUNT(*)
    FROM Produits
    WHERE idAdmin = @idAdmin AND statut = 'active';

    -- Calcul des pourcentages de changement
    DECLARE @SalesChangePercent DECIMAL(5,2);
    IF @LastMonthSales > 0
        SET @SalesChangePercent = ((@CurrentMonthSales - @LastMonthSales) / @LastMonthSales) * 100;
    ELSE IF @CurrentMonthSales > 0
        SET @SalesChangePercent = 100;
    ELSE
        SET @SalesChangePercent = 0;

    DECLARE @OrdersChangePercent DECIMAL(5,2);
    IF @LastMonthOrders > 0
        SET @OrdersChangePercent = ((@CurrentMonthOrders - @LastMonthOrders) * 1.0 / @LastMonthOrders) * 100;
    ELSE IF @CurrentMonthOrders > 0
        SET @OrdersChangePercent = 100;
    ELSE
        SET @OrdersChangePercent = 0;

    -- Retourner les résultats
    SELECT 
        @CurrentMonthSales AS VentesMois,
        @SalesChangePercent AS VentesPourcentage,
        @CurrentMonthOrders AS CommandesMois,
        @OrdersChangePercent AS CommandesPourcentage,
        @ActiveProducts AS ProduitsActifs;
END;
GO
