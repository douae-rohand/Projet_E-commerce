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

GO

USE [cooperative]
GO

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

        -- Utiliser une variable table pour stocker les candidats temporairement
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

        -- Décrémenter le stock des variantes concernées
        UPDATE v
        SET v.quantite = v.quantite - lc.quantite,
            v.updated_at = GETDATE()
        FROM [dbo].[Variantes] v
        INNER JOIN [dbo].[LignesCommande] lc ON v.idV = lc.idV
        INNER JOIN [dbo].[Produits] p ON v.idP = p.idP
        INNER JOIN @Candidates c ON c.idCommande = lc.idCommande
        WHERE p.idAdmin = @idAdmin;

        -- Mettre à jour les commandes
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

GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_create_product')
    DROP PROCEDURE sp_create_product;
GO

CREATE PROCEDURE sp_create_product
    @idAdmin INT,
    @nomP NVARCHAR(255),
    @description NVARCHAR(MAX) = NULL,
    @seuil_alerte INT = 10,
    @idC INT,
    -- Première variante
    @prix DECIMAL(10,2),
    @taille NVARCHAR(20) = NULL,
    @couleur NVARCHAR(20) = NULL,
    @photo NVARCHAR(255) = NULL,
    @quantite INT,
    @poids NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Insérer le produit
        INSERT INTO Produits (nomP, description, seuil_alerte, statut, idC, idAdmin, created_at, updated_at)
        VALUES (@nomP, @description, @seuil_alerte, 'active', @idC, @idAdmin, GETDATE(), GETDATE());

        DECLARE @idP INT = SCOPE_IDENTITY();

        -- Insérer la première variante
        INSERT INTO Variantes (idP, prix, taille, couleur, photo, quantite, poids, created_at, updated_at)
        VALUES (@idP, @prix, @taille, @couleur, @photo, @quantite, @poids, GETDATE(), GETDATE());

        COMMIT TRANSACTION;

        -- Retourner l'ID du produit créé
        SELECT @idP AS idP, 'PRODUCT_CREATED' AS message;
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

GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_delete_product')
    DROP PROCEDURE sp_delete_product;
GO

CREATE PROCEDURE sp_delete_product
    @idP INT,
    @idAdmin INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Vérifier que le produit appartient à l'admin
        IF NOT EXISTS (SELECT 1 FROM Produits WHERE idP = @idP AND idAdmin = @idAdmin)
        BEGIN
            RAISERROR('Produit non trouvé ou accès non autorisé', 16, 1);
            RETURN;
        END

        -- Soft delete: changer le statut à 'inactive'
        UPDATE Produits
        SET 
            statut = 'inactive',
            updated_at = GETDATE()
        WHERE idP = @idP AND idAdmin = @idAdmin;

        SELECT 'PRODUCT_DELETED' AS message;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000);
        SET @ErrorMessage = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO

GO

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

GO

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

GO

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

GO

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

GO

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
        lc.prix_unitaire AS PrixUnitaire,
        v.photo AS Photo
        -- SousTotal calculé côté C#
    FROM LignesCommande lc
    INNER JOIN Variantes v ON lc.idV = v.idV
    INNER JOIN Produits p ON v.idP = p.idP
    WHERE lc.idCommande = @idCommande
        AND p.idAdmin = @idAdmin;
END;
GO

GO

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

GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_inscription_client')
    DROP PROCEDURE sp_inscription_client;
GO

CREATE PROCEDURE sp_inscription_client
    @email NVARCHAR(100),
    @password NVARCHAR(255),
    @prenom NVARCHAR(255),
    @nom NVARCHAR(255),
    @telephone NVARCHAR(100),
    @date_naissance DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Vérifier si l'email existe déjà
        IF EXISTS (SELECT 1 FROM Utilisateurs WHERE email = @email)
        BEGIN
            RAISERROR('Email déjà utilisé', 16, 1);
            RETURN;
        END

        -- Insertion dans la table Utilisateurs
        INSERT INTO Utilisateurs (email, password, est_actif, created_at, updated_at)
        VALUES (@email, @password, 1, GETDATE(), GETDATE());

        DECLARE @userId INT;
        SET @userId = SCOPE_IDENTITY();

        -- Insertion dans la table Clients
        INSERT INTO Clients (
            id,
            prenom,
            nom,
            telephone,
            date_naissance,
            created_at,
            updated_at
        )
        VALUES (
            @userId,
            @prenom,
            @nom,
            @telephone,
            @date_naissance,
            GETDATE(),
            GETDATE()
        );

        COMMIT TRANSACTION;

        -- Retourner un message de succès
        SELECT 'INSCRIPTION_CLIENT_OK' AS message, @userId AS id;
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

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_login_utilisateur')
    DROP PROCEDURE sp_login_utilisateur;
GO

CREATE PROCEDURE sp_login_utilisateur
    @email NVARCHAR(100),
    @password NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    -- Vérifier si l'utilisateur existe avec les bonnes credentials
    IF NOT EXISTS (
        SELECT 1 
        FROM Utilisateurs
        WHERE email = @email
          AND password = @password
          AND est_actif = 1
    )
    BEGIN
        RAISERROR('Email ou mot de passe incorrect', 16, 1);
        RETURN;
    END

    DECLARE @userId INT;

    -- Récupérer l'ID de l'utilisateur
    SELECT @userId = id
    FROM Utilisateurs
    WHERE email = @email;

    -- Déterminer le rôle et retourner les informations
    IF EXISTS (SELECT 1 FROM SuperAdmins WHERE id = @userId)
    BEGIN
        SELECT id, email, 'SUPER_ADMIN' AS role 
        FROM Utilisateurs 
        WHERE id = @userId;
    END
    ELSE IF EXISTS (SELECT 1 FROM Admins WHERE id = @userId)
    BEGIN
        SELECT id, email, 'ADMIN' AS role 
        FROM Utilisateurs 
        WHERE id = @userId;
    END
    ELSE IF EXISTS (SELECT 1 FROM Clients WHERE id = @userId)
    BEGIN
        SELECT id, email, 'CLIENT' AS role 
        FROM Utilisateurs 
        WHERE id = @userId;
    END
    ELSE
    BEGIN
        RAISERROR('Utilisateur sans rôle défini', 16, 1);
        RETURN;
    END
END;

GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_update_admin_profile')
    DROP PROCEDURE sp_update_admin_profile;
GO

CREATE PROCEDURE sp_update_admin_profile
    @idAdmin INT,
    @nom_cooperative NVARCHAR(255),
    @localisation NVARCHAR(MAX) = NULL,
    @ville NVARCHAR(255) = NULL,
    @telephone NVARCHAR(100) = NULL,
    @description NVARCHAR(MAX) = NULL,
    @logo NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Vérifier que l'admin existe
        IF NOT EXISTS (SELECT 1 FROM Admins WHERE id = @idAdmin)
        BEGIN
            RAISERROR('Admin non trouvé', 16, 1);
            RETURN;
        END

        -- Mettre à jour le profil admin
        UPDATE Admins
        SET 
            nom_cooperative = @nom_cooperative,
            localisation = @localisation,
            ville = @ville,
            telephone = @telephone,
            description = @description,
            logo = ISNULL(@logo, logo),  -- Ne mettre à jour que si un nouveau logo est fourni
            updated_at = GETDATE()
        WHERE id = @idAdmin;

        SELECT 'PROFILE_UPDATED' AS message;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000);
        SET @ErrorMessage = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO

GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_update_product')
    DROP PROCEDURE sp_update_product;
GO

CREATE PROCEDURE sp_update_product
    @idP INT,
    @idAdmin INT,
    @nomP NVARCHAR(255),
    @description NVARCHAR(MAX) = NULL,
    @seuil_alerte INT,
    @idC INT,
    @statut NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Vérifier que le produit appartient à l'admin
        IF NOT EXISTS (SELECT 1 FROM Produits WHERE idP = @idP AND idAdmin = @idAdmin)
        BEGIN
            RAISERROR('Produit non trouvé ou accès non autorisé', 16, 1);
            RETURN;
        END

        -- Mettre à jour le produit
        UPDATE Produits
        SET 
            nomP = @nomP,
            description = @description,
            seuil_alerte = @seuil_alerte,
            idC = @idC,
            statut = @statut,
            updated_at = GETDATE()
        WHERE idP = @idP AND idAdmin = @idAdmin;

        SELECT 'PRODUCT_UPDATED' AS message;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000);
        SET @ErrorMessage = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO

GO

