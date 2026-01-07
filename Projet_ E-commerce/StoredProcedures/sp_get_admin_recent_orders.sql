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
            WHEN c.statut = 'en_preparation' THEN 'bg-info-subtle text-info'
            WHEN c.statut = 'en_livraison' THEN 'bg-primary-subtle text-primary'
            WHEN c.statut = 'livre' THEN 'bg-success-subtle text-success'
            WHEN c.statut = 'annule' THEN 'bg-danger-subtle text-danger'
            ELSE 'bg-secondary-subtle text-secondary'
        END AS StatusClass,
        CASE 
            WHEN c.statut = 'en_attente' THEN 'En attente'
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
