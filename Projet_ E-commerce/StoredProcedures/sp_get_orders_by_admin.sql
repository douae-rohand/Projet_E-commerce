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
        -- Informations de livraison
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
        AND (@idCommande IS NULL OR c.idCommande = @idCommande)
    ORDER BY c.created_at DESC;
END;
GO
