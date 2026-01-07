USE [cooperative];
GO

-- =============================================
-- 1. MIGRATION DU SCHÉMA (TABLES & CONTRAINTES)
-- =============================================

-- Mettre à jour les status 'valide' -> 'livre' 
UPDATE Commandes
SET statut = 'livre'
WHERE statut = 'valide';
GO

-- Supprimer l'ancienne contrainte CHK_Commandes_statut
IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CHK_Commandes_statut')
BEGIN
    ALTER TABLE Commandes
    DROP CONSTRAINT CHK_Commandes_statut;
END
GO

-- Ajouter la nouvelle contrainte incluant 'en_preparation', 'en_livraison', 'livre'
ALTER TABLE Commandes
ADD CONSTRAINT CHK_Commandes_statut 
CHECK (statut IN ('annule', 'livre', 'en_livraison', 'en_preparation', 'en_attente'));
GO

-- Supprimer la colonne statut de la table Livraisons (et sa contrainte associée)
IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Livraisons_Statut')
BEGIN
    ALTER TABLE Livraisons
    DROP CONSTRAINT CK_Livraisons_Statut;
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'statut' AND Object_ID = Object_ID(N'Livraisons'))
BEGIN
    ALTER TABLE Livraisons
    DROP COLUMN statut;
END
GO

-- =============================================
-- 2. MISE À JOUR DES PROCÉDURES STOCKÉES
-- =============================================

-- A. sp_get_orders_by_admin
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

-- B. sp_get_admin_recent_orders
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

-- C. sp_accept_order (Mise à jour: valide -> en_preparation)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_accept_order')
    DROP PROCEDURE sp_accept_order;
GO

CREATE PROCEDURE sp_accept_order
    @idCommande INT,
    @idAdmin INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        -- Vérifier que la commande contient des produits de cet admin
        IF NOT EXISTS (
            SELECT 1 
            FROM Commandes c
            INNER JOIN LignesCommande lc ON c.idCommande = lc.idCommande
            INNER JOIN Variantes v ON lc.idV = v.idV
            INNER JOIN Produits p ON v.idP = p.idP
            WHERE c.idCommande = @idCommande AND p.idAdmin = @idAdmin
        )
        BEGIN
            RAISERROR('Commande non trouvée ou accès non autorisé', 16, 1);
            RETURN;
        END

        -- Vérifier que la commande est en attente
        IF NOT EXISTS (SELECT 1 FROM Commandes WHERE idCommande = @idCommande AND statut = 'en_attente')
        BEGIN
            RAISERROR('La commande n''est pas en attente de validation', 16, 1);
            RETURN;
        END

        BEGIN TRANSACTION;

        -- Vérifier le stock pour chaque ligne de commande de cet admin
        IF EXISTS (
            SELECT 1
            FROM LignesCommande lc
            INNER JOIN Variantes v ON lc.idV = v.idV
            INNER JOIN Produits p ON v.idP = p.idP
            WHERE lc.idCommande = @idCommande
              AND p.idAdmin = @idAdmin
              AND lc.quantite > v.quantite
        )
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('STOCK_INSUFFICIENT', 16, 1);
            RETURN;
        END

        -- Décrémenter le stock des variantes concernées
        UPDATE v
        SET v.quantite = v.quantite - lc.quantite,
            v.updated_at = GETDATE()
        FROM Variantes v
        INNER JOIN LignesCommande lc ON v.idV = lc.idV
        INNER JOIN Produits p ON v.idP = p.idP
        WHERE lc.idCommande = @idCommande
          AND p.idAdmin = @idAdmin;

        -- Mettre à jour le statut de la commande
        UPDATE Commandes
        SET 
            statut = 'en_preparation',
            updated_at = GETDATE()
        WHERE idCommande = @idCommande;

        COMMIT TRANSACTION;

        SELECT 'ORDER_ACCEPTED' AS message;
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

-- D. sp_auto_accept_orders_by_admin (Mise à jour: valide -> en_preparation)
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

        UPDATE v
        SET v.quantite = v.quantite - lc.quantite,
            v.updated_at = GETDATE()
        FROM [dbo].[Variantes] v
        INNER JOIN [dbo].[LignesCommande] lc ON v.idV = lc.idV
        INNER JOIN [dbo].[Produits] p ON v.idP = p.idP
        INNER JOIN @Candidates c ON c.idCommande = lc.idCommande
        WHERE p.idAdmin = @idAdmin;

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
