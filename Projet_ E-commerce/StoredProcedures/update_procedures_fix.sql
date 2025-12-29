-- Script de mise à jour des procédures stockées
-- Exécutez ce script dans SQL Server Management Studio pour corriger les noms de colonnes
-- Database: cooperative

USE [cooperative]
GO

PRINT 'Mise à jour des procédures stockées...';
GO

-- Supprimer et recréer sp_get_products_by_admin
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
        (SELECT COUNT(*) FROM Variantes WHERE idP = p.idP) AS NombreVariantes,
        ISNULL((SELECT SUM(quantite) FROM Variantes WHERE idP = p.idP), 0) AS StockTotal,
        (SELECT MIN(prix) FROM Variantes WHERE idP = p.idP) AS PrixMin,
        (SELECT MAX(prix) FROM Variantes WHERE idP = p.idP) AS PrixMax
    FROM Produits p
    INNER JOIN Categories c ON p.idC = c.idC
    WHERE p.idAdmin = @idAdmin
    ORDER BY p.created_at DESC;
END;
GO

PRINT 'sp_get_products_by_admin mis à jour';
GO

-- Supprimer et recréer sp_get_orders_by_admin
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_get_orders_by_admin')
    DROP PROCEDURE sp_get_orders_by_admin;
GO

CREATE PROCEDURE sp_get_orders_by_admin
    @idAdmin INT,
    @statut NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        c.idCommande AS IdCommande,
        CONCAT('#CO-', YEAR(c.created_at), '-', FORMAT(c.idCommande, '000')) AS NumeroCommande,
        c.idClient AS IdClient,
        CONCAT(cl.prenom, ' ', cl.nom) AS NomClient,
        cl.telephone AS TelephoneClient,
        c.statut AS Statut,
        c.prixTotal AS PrixTotal,
        c.created_at AS CreatedAt,
        c.updated_at AS UpdatedAt,
        CASE 
            WHEN c.statut = 'en_attente' THEN 'En attente'
            WHEN c.statut = 'valide' THEN 'Validée'
            WHEN c.statut = 'annule' THEN 'Annulée'
            ELSE c.statut
        END AS StatusLabel,
        CASE 
            WHEN c.statut = 'en_attente' THEN 'bg-warning-subtle text-warning'
            WHEN c.statut = 'valide' THEN 'bg-success-subtle text-success'
            WHEN c.statut = 'annule' THEN 'bg-danger-subtle text-danger'
            ELSE 'bg-secondary-subtle text-secondary'
        END AS StatusClass,
        l.statut AS StatutLivraison,
        l.mode_livraison AS ModeLivraison,
        l.dateDebutEstimation AS DateDebutEstimation,
        l.dateFinEstimation AS DateFinEstimation
    FROM Commandes c
    INNER JOIN Clients cl ON c.idClient = cl.id
    INNER JOIN LignesCommande lc ON c.idCommande = lc.idCommande
    INNER JOIN Variantes v ON lc.idV = v.idV
    INNER JOIN Produits p ON v.idP = p.idP
    LEFT JOIN Livraisons l ON c.idCommande = l.idCommande
    WHERE p.idAdmin = @idAdmin
        AND (@statut IS NULL OR c.statut = @statut)
    ORDER BY c.created_at DESC;
END;
GO

PRINT 'sp_get_orders_by_admin mis à jour';
GO

-- Supprimer et recréer sp_get_admin_profile
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_get_admin_profile')
    DROP PROCEDURE sp_get_admin_profile;
GO

CREATE PROCEDURE sp_get_admin_profile
    @idAdmin INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        a.id AS Id,
        u.email AS Email,
        a.nom_cooperative AS NomCooperative,
        a.localisation AS Localisation,
        a.ville AS Ville,
        a.logo AS Logo,
        a.telephone AS Telephone,
        a.description AS Description,
        a.created_at AS CreatedAt,
        a.updated_at AS UpdatedAt,
        u.est_actif AS EstActif
    FROM Admins a
    INNER JOIN Utilisateurs u ON a.id = u.id
    WHERE a.id = @idAdmin;
END;
GO

PRINT 'sp_get_admin_profile mis à jour';
GO

-- Supprimer et recréer sp_get_admin_top_products
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

PRINT 'sp_get_admin_top_products mis à jour';
GO

-- Supprimer et recréer sp_get_admin_recent_orders
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_get_admin_recent_orders')
    DROP PROCEDURE sp_get_admin_recent_orders;
GO

CREATE PROCEDURE sp_get_admin_recent_orders
    @idAdmin INT,
    @limit INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@limit)
        c.idCommande AS IdCommande,
        CONCAT('#CO-', YEAR(c.created_at), '-', FORMAT(c.idCommande, '000')) AS NumeroCommande,
        CONCAT(cl.prenom, ' ', cl.nom) AS NomClient,
        c.statut AS Statut,
        c.prixTotal AS PrixTotal,
        c.created_at AS CreatedAt,
        CASE 
            WHEN c.statut = 'en_attente' THEN 'bg-warning-subtle text-warning'
            WHEN c.statut = 'valide' THEN 'bg-primary-subtle text-primary'
            WHEN c.statut = 'annule' THEN 'bg-danger-subtle text-danger'
            ELSE 'bg-secondary-subtle text-secondary'
        END AS StatusClass,
        CASE 
            WHEN c.statut = 'en_attente' THEN 'En attente'
            WHEN c.statut = 'valide' THEN 'Validée'
            WHEN c.statut = 'annule' THEN 'Annulée'
            ELSE c.statut
        END AS StatusLabel
    FROM Commandes c
    INNER JOIN Clients cl ON c.idClient = cl.id
    INNER JOIN LignesCommande lc ON c.idCommande = lc.idCommande
    INNER JOIN Variantes v ON lc.idV = v.idV
    INNER JOIN Produits p ON v.idP = p.idP
    WHERE p.idAdmin = @idAdmin
    GROUP BY c.idCommande, c.created_at, c.statut, c.prixTotal, cl.prenom, cl.nom
    ORDER BY c.created_at DESC;
END;
GO

PRINT 'sp_get_admin_recent_orders mis à jour';
GO

PRINT '✓ Toutes les procédures stockées ont été mises à jour avec succès!';
PRINT 'Les noms de colonnes correspondent maintenant aux propriétés C#.';
GO
