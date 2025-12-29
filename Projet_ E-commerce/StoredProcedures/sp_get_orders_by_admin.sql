USE [cooperative]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_get_orders_by_admin')
    DROP PROCEDURE sp_get_orders_by_admin;
GO

CREATE PROCEDURE sp_get_orders_by_admin
    @idAdmin INT,
    @statut NVARCHAR(20) = NULL,  -- Filtre optionnel par statut
    @idCommande INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH OrdersWithAdminLines AS (
        SELECT 
            c.idCommande,
            SUM(lc.quantite * lc.prix_unitaire) AS PrixTotalAdmin,
            MIN(v.photo) AS Thumbnail
        FROM [dbo].[Commandes] c
        INNER JOIN [dbo].[LignesCommande] lc ON c.idCommande = lc.idCommande
        INNER JOIN [dbo].[Variantes] v ON lc.idV = v.idV
        INNER JOIN [dbo].[Produits] p ON v.idP = p.idP
        WHERE p.idAdmin = @idAdmin
        GROUP BY c.idCommande
    )
    SELECT 
        c.idCommande AS IdCommande,
        CONCAT('#CO-', YEAR(c.created_at), '-', RIGHT('000' + CAST(c.idCommande AS VARCHAR(10)), 3)) AS NumeroCommande,
        c.idClient AS IdClient,
        CONCAT(cl.prenom, ' ', cl.nom) AS NomClient,
        cl.telephone AS TelephoneClient,
        c.statut AS Statut,
        c.prixTotal AS PrixTotal,
        ISNULL(owa.PrixTotalAdmin, 0) AS PrixTotalAdmin,
        owa.Thumbnail AS Thumbnail,
        c.created_at AS CreatedAt,
        c.updated_at AS UpdatedAt,
        CASE 
            WHEN c.statut = 'en_attente' THEN 'En attente'
            WHEN c.statut = 'en_preparation' THEN 'En préparation'
            WHEN c.statut = 'en_livraison' THEN 'En livraison'
            WHEN c.statut = 'livre' THEN 'Livrée'
            WHEN c.statut = 'annule' THEN 'Annulée'
            ELSE c.statut
        END AS StatusLabel,
        CASE 
            WHEN c.statut = 'en_attente' THEN 'bg-warning-subtle text-warning'
            WHEN c.statut = 'en_preparation' THEN 'bg-info-subtle text-info'
            WHEN c.statut = 'en_livraison' THEN 'bg-primary-subtle text-primary'
            WHEN c.statut = 'livre' THEN 'bg-success-subtle text-success'
            WHEN c.statut = 'annule' THEN 'bg-danger-subtle text-danger'
            ELSE 'bg-secondary-subtle text-secondary'
        END AS StatusClass,
        -- Informations de livraison (statut supprimé de la table Livraisons)
        l.mode_livraison AS ModeLivraison,
        l.dateDebutEstimation AS DateDebutEstimation,
        l.dateFinEstimation AS DateFinEstimation
    FROM OrdersWithAdminLines owa
    INNER JOIN [dbo].[Commandes] c ON owa.idCommande = c.idCommande
    INNER JOIN [dbo].[Clients] cl ON c.idClient = cl.id
    LEFT JOIN [dbo].[Livraisons] l ON c.idCommande = l.idCommande
    WHERE (@statut IS NULL OR c.statut = @statut)
        AND (@idCommande IS NULL OR c.idCommande = @idCommande)
    ORDER BY c.created_at DESC;
END;
GO
