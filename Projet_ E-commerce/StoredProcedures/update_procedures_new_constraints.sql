-- Script de mise à jour des procédures stockées suite aux changements de contraintes de statut
USE [cooperative]
GO

PRINT 'Mise à jour de sp_get_orders_by_admin...';
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_get_orders_by_admin')
    DROP PROCEDURE sp_get_orders_by_admin;
GO

CREATE PROCEDURE sp_get_orders_by_admin
    @idAdmin INT,
    @statut NVARCHAR(20) = NULL,
    @idCommande INT = NULL
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
            WHEN c.statut = 'en_preparation' THEN 'En préparation'
            WHEN c.statut = 'en_livraison' THEN 'En livraison'
            WHEN c.statut = 'livre' THEN 'Livrée'
            WHEN c.statut = 'annule' THEN 'Annulée'
            ELSE c.statut
        END AS StatusLabel,
        CASE 
            WHEN c.statut = 'en_attente' THEN 'bg-warning-subtle text-warning'
            WHEN c.statut = 'valide' THEN 'bg-success-subtle text-success'
            WHEN c.statut = 'en_preparation' THEN 'bg-info-subtle text-info'
            WHEN c.statut = 'en_livraison' THEN 'bg-primary-subtle text-primary'
            WHEN c.statut = 'livre' THEN 'bg-success text-white'
            WHEN c.statut = 'annule' THEN 'bg-danger-subtle text-danger'
            ELSE 'bg-secondary-subtle text-secondary'
        END AS StatusClass,
        -- Informations de livraison
        CASE 
            WHEN l.statut = 'en_preparation' THEN 'En préparation'
            WHEN l.statut = 'en_cours' THEN 'En cours'
            WHEN l.statut = 'livre' THEN 'Livrée'
            WHEN l.statut = 'non_livre' THEN 'Non livrée'
            ELSE l.statut
        END AS StatutLivraison,
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
        AND (@idCommande IS NULL OR c.idCommande = @idCommande)
    ORDER BY c.created_at DESC;
END;
GO

PRINT 'Mise à jour de sp_get_admin_recent_orders...';
GO

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
            WHEN c.statut = 'valide' THEN 'bg-success-subtle text-success'
            WHEN c.statut = 'en_preparation' THEN 'bg-info-subtle text-info'
            WHEN c.statut = 'en_livraison' THEN 'bg-primary-subtle text-primary'
            WHEN c.statut = 'livre' THEN 'bg-success text-white'
            WHEN c.statut = 'annule' THEN 'bg-danger-subtle text-danger'
            ELSE 'bg-secondary-subtle text-secondary'
        END AS StatusClass,
        CASE 
            WHEN c.statut = 'en_attente' THEN 'En attente'
            WHEN c.statut = 'valide' THEN 'Validée'
            WHEN c.statut = 'en_preparation' THEN 'En préparation'
            WHEN c.statut = 'en_livraison' THEN 'En livraison'
            WHEN c.statut = 'livre' THEN 'Livrée'
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

PRINT 'Procédures stockées mises à jour avec succès.';
