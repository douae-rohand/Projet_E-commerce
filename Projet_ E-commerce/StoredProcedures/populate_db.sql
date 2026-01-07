-- ============================================
-- SCRIPT POUR VIDER ET REMPLIR LA BASE DE DONNÉES
-- AVEC MOTS DE PASSE HASHÉS POUR LES TESTS
-- ============================================

-- ============================================
-- ÉTAPE 1: VIDER TOUTES LES TABLES
-- ============================================

-- Supprimer les données dans l'ordre correct
DELETE FROM BordereauxLivraison;
DELETE FROM Livraisons;
DELETE FROM Factures;
DELETE FROM LignesCommande;
DELETE FROM Variantes;
DELETE FROM Avis;
DELETE FROM Produits;
DELETE FROM AdressesLivraison;
DELETE FROM Commandes;
DELETE FROM Clients;
DELETE FROM Admins;
DELETE FROM SuperAdmins;
DELETE FROM Utilisateurs;
DELETE FROM Categories;

-- Réinitialiser les compteurs d'identité
DBCC CHECKIDENT ('BordereauxLivraison', RESEED, 0);
DBCC CHECKIDENT ('Livraisons', RESEED, 0);
DBCC CHECKIDENT ('Factures', RESEED, 0);
DBCC CHECKIDENT ('LignesCommande', RESEED, 0);
DBCC CHECKIDENT ('Variantes', RESEED, 0);
DBCC CHECKIDENT ('Avis', RESEED, 0);
DBCC CHECKIDENT ('Produits', RESEED, 0);
DBCC CHECKIDENT ('AdressesLivraison', RESEED, 0);
DBCC CHECKIDENT ('Commandes', RESEED, 0);
DBCC CHECKIDENT ('Utilisateurs', RESEED, 0);
DBCC CHECKIDENT ('Categories', RESEED, 0);

-- ============================================
-- ÉTAPE 2: REMPLIR LA BASE DE DONNÉES
-- ============================================

-- 1. CATEGORIES
INSERT INTO Categories (nom, description, created_at, updated_at) VALUES
('cosmetique', 'Produits cosmétiques et de beauté', GETDATE(), GETDATE()),
('nutritive', 'Produits nutritionnels et alimentaires', GETDATE(), GETDATE()),
('medical', 'Produits médicaux et de santé', GETDATE(), GETDATE()),
('artisanat', 'Produits artisanaux locaux', GETDATE(), GETDATE()),
('agricole', 'Produits agricoles et fermiers', GETDATE(), GETDATE());

-- 2. UTILISATEURS (MOTS DE PASSE HASHÉS POUR LES TESTS)
INSERT INTO Utilisateurs (email, est_actif, password, created_at, updated_at) VALUES
('admin1@coop-argan.ma', 1, CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', 'admin123'), 2), GETDATE(), GETDATE()),
('admin2@coop-olive.ma', 1, CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', 'admin123'), 2), GETDATE(), GETDATE()),
('admin3@coop-miel.ma', 1, CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', 'admin123'), 2), GETDATE(), GETDATE()),
('superadmin@platform.ma', 1, CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', 'super123'), 2), GETDATE(), GETDATE()),
('client1@email.com', 1, CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', 'client123'), 2), GETDATE(), GETDATE()),
('client2@email.com', 1, CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', 'client123'), 2), GETDATE(), GETDATE()),
('client3@email.com', 1, CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', 'client123'), 2), GETDATE(), GETDATE()),
('client4@email.com', 1, CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', 'client123'), 2), GETDATE(), GETDATE());

-- 3. ADMINS (COOPÉRATIVES)
INSERT INTO Admins (id, nom_cooperative, localisation, ville, logo, telephone, created_at, updated_at) VALUES
(1, 'Coopérative Argan du Sud', 'Route d''Agadir, Km 12', 'Agadir', 'logo_argan.png', '+212-661-234567', GETDATE(), GETDATE()),
(2, 'Coopérative Olive du Nord', 'Zone Industrielle', 'Meknès', 'logo_olive.png', '+212-662-345678', GETDATE(), GETDATE()),
(3, 'Coopérative Miel Atlas', 'Douar Ait Baha', 'Azilal', 'logo_miel.png', '+212-663-456789', GETDATE(), GETDATE());

-- 4. SUPERADMIN
INSERT INTO SuperAdmins (id) VALUES (4);

-- 5. CLIENTS
INSERT INTO Clients (id, prenom, nom, telephone, date_naissance, created_at, updated_at) VALUES
(5, 'Fatima', 'Benali', '+212-664-111111', '1990-05-15', GETDATE(), GETDATE()),
(6, 'Mohammed', 'Alami', '+212-665-222222', '1985-08-22', GETDATE(), GETDATE()),
(7, 'Amina', 'Tazi', '+212-666-333333', '1992-03-10', GETDATE(), GETDATE()),
(8, 'Youssef', 'Idrissi', '+212-667-444444', '1988-11-30', GETDATE(), GETDATE());

-- 6. ADRESSES DE LIVRAISON
INSERT INTO AdressesLivraison (idClient, nom_adresse, adresse_complete, ville, code_postal, telephone, est_par_defaut, created_at) VALUES
(5, 'Domicile', '45 Avenue Mohammed V, Appartement 12', 'Casablanca', '20000', '+212-664-111111', 1, GETDATE()),
(5, 'Bureau', 'Twin Center, Tour A, Bureau 503', 'Casablanca', '20100', '+212-664-111111', 0, GETDATE()),
(6, 'Maison', '78 Rue Oued Sebou', 'Rabat', '10000', '+212-665-222222', 1, GETDATE()),
(7, 'Domicile', '23 Boulevard Hassan II', 'Marrakech', '40000', '+212-666-333333', 1, GETDATE()),
(8, 'Maison', '156 Avenue des FAR', 'Tanger', '90000', '+212-667-444444', 1, GETDATE());

-- 7. PRODUITS
INSERT INTO Produits (nomP, description, seuil_alerte, statut, idC, idAdmin, created_at, updated_at) VALUES
('Huile d''Argan Pure Bio', 'Huile d''argan 100% pure et biologique, pressée à froid', 10, 'active', 1, 1, GETDATE(), GETDATE()),
('Savon Noir à l''Argan', 'Savon noir traditionnel enrichi à l''huile d''argan', 20, 'active', 1, 1, GETDATE(), GETDATE()),
('Crème Visage Argan', 'Crème hydratante pour le visage à base d''huile d''argan', 15, 'active', 1, 1, GETDATE(), GETDATE()),
('Huile d''Olive Extra Vierge', 'Huile d''olive extra vierge première pression à froid', 15, 'active', 2, 2, GETDATE(), GETDATE()),
('Olives Vertes Marinées', 'Olives vertes marinées aux herbes aromatiques', 25, 'active', 2, 2, GETDATE(), GETDATE()),
('Tapenade d''Olives', 'Tapenade artisanale aux olives noires', 10, 'active', 2, 2, GETDATE(), GETDATE()),
('Miel de Thym Pur', 'Miel de thym 100% naturel récolté dans l''Atlas', 12, 'active', 3, 3, GETDATE(), GETDATE()),
('Miel d''Euphorbe', 'Miel rare d''euphorbe, propriétés thérapeutiques', 8, 'active', 3, 3, GETDATE(), GETDATE()),
('Pollen d''Abeilles', 'Pollen d''abeilles séché, riche en nutriments', 20, 'active', 2, 3, GETDATE(), GETDATE()),
('Gelée Royale', 'Gelée royale fraîche, source de vitalité', 5, 'active', 3, 3, GETDATE(), GETDATE());

-- 8. VARIANTES
INSERT INTO Variantes (idP, prix, taille, couleur, photo, quantite, poids, created_at, updated_at) VALUES
(1, 150.00, '50ml', NULL, 'argan_50ml.jpg', 50, '50ml', GETDATE(), GETDATE()),
(1, 280.00, '100ml', NULL, 'argan_100ml.jpg', 35, '100ml', GETDATE(), GETDATE()),
(1, 500.00, '250ml', NULL, 'argan_250ml.jpg', 20, '250ml', GETDATE(), GETDATE()),
(2, 45.00, NULL, NULL, 'savon_noir.jpg', 100, '200g', GETDATE(), GETDATE()),
(3, 220.00, '50ml', NULL, 'creme_visage.jpg', 40, '50ml', GETDATE(), GETDATE()),
(4, 80.00, '500ml', NULL, 'olive_500ml.jpg', 60, '500ml', GETDATE(), GETDATE()),
(4, 145.00, '1L', NULL, 'olive_1l.jpg', 45, '1L', GETDATE(), GETDATE()),
(5, 35.00, NULL, 'Vert', 'olives_vertes.jpg', 80, '250g', GETDATE(), GETDATE()),
(6, 55.00, NULL, NULL, 'tapenade.jpg', 50, '200g', GETDATE(), GETDATE()),
(7, 120.00, '250g', NULL, 'miel_thym_250g.jpg', 30, '250g', GETDATE(), GETDATE()),
(7, 220.00, '500g', NULL, 'miel_thym_500g.jpg', 25, '500g', GETDATE(), GETDATE()),
(8, 180.00, '250g', NULL, 'miel_euphorbe.jpg', 15, '250g', GETDATE(), GETDATE()),
(9, 95.00, NULL, NULL, 'pollen.jpg', 40, '100g', GETDATE(), GETDATE()),
(10, 350.00, NULL, NULL, 'gelee_royale.jpg', 10, '50g', GETDATE(), GETDATE());

-- 9. COMMANDES
INSERT INTO Commandes (idClient, statut, prixTotal, created_at, updated_at) VALUES
(5, 'valide', 605.00, DATEADD(day, -15, GETDATE()), DATEADD(day, -14, GETDATE())),
(6, 'valide', 355.00, DATEADD(day, -10, GETDATE()), DATEADD(day, -9, GETDATE())),
(7, 'en_attente', 300.00, DATEADD(day, -3, GETDATE()), DATEADD(day, -3, GETDATE())),
(8, 'valide', 660.00, DATEADD(day, -7, GETDATE()), DATEADD(day, -6, GETDATE())),
(5, 'annule', 220.00, DATEADD(day, -20, GETDATE()), DATEADD(day, -19, GETDATE()));

-- 10. LIGNES DE COMMANDE
INSERT INTO LignesCommande (idCommande, idV, quantite, prix_unitaire, created_at) VALUES
(1, 2, 2, 280.00, DATEADD(day, -15, GETDATE())),
(1, 4, 1, 45.00, DATEADD(day, -15, GETDATE())),
(2, 10, 2, 120.00, DATEADD(day, -10, GETDATE())),
(2, 13, 1, 95.00, DATEADD(day, -10, GETDATE())),
(2, 9, 1, 55.00, DATEADD(day, -10, GETDATE())),
(3, 1, 2, 150.00, DATEADD(day, -3, GETDATE())),
(4, 3, 1, 500.00, DATEADD(day, -7, GETDATE())),
(4, 6, 2, 80.00, DATEADD(day, -7, GETDATE())),
(5, 5, 1, 220.00, DATEADD(day, -20, GETDATE()));

-- 11. LIVRAISONS
INSERT INTO Livraisons (idCommande, idAdresse, statut, dateDebutEstimation, dateFinEstimation, notes, frais, mode_livraison, created_at, updated_at) VALUES
(1, 1, 'livre', DATEADD(day, -12, GETDATE()), DATEADD(day, -10, GETDATE()), 'Livraison effectuée sans problème', 30.00, 'Standard', DATEADD(day, -14, GETDATE()), DATEADD(day, -10, GETDATE())),
(2, 3, 'en_cours', DATEADD(day, -7, GETDATE()), DATEADD(day, 1, GETDATE()), NULL, 40.00, 'Express', DATEADD(day, -9, GETDATE()), DATEADD(day, -5, GETDATE())),
(3, 4, 'non_livre', DATEADD(day, 2, GETDATE()), DATEADD(day, 5, GETDATE()), 'En attente de validation de la commande', 35.00, 'Standard', DATEADD(day, -3, GETDATE()), DATEADD(day, -3, GETDATE())),
(4, 5, 'livre', DATEADD(day, -5, GETDATE()), DATEADD(day, -3, GETDATE()), 'Client très satisfait', 50.00, 'Express', DATEADD(day, -6, GETDATE()), DATEADD(day, -3, GETDATE())),
(5, 2, 'non_livre', DATEADD(day, -18, GETDATE()), DATEADD(day, -16, GETDATE()), 'Commande annulée par le client', 25.00, 'Standard', DATEADD(day, -19, GETDATE()), DATEADD(day, -19, GETDATE()));

-- 12. BORDEREAUX DE LIVRAISON
INSERT INTO BordereauxLivraison (idLivraison, numero_bordereau, date_generation, path_bordereau) VALUES
(1, 'BL-2024-001', DATEADD(day, -14, GETDATE()), 'bordereaux/BL-2024-001.pdf'),
(2, 'BL-2024-002', DATEADD(day, -9, GETDATE()), 'bordereaux/BL-2024-002.pdf'),
(3, 'BL-2024-003', DATEADD(day, -3, GETDATE()), 'bordereaux/BL-2024-003.pdf'),
(4, 'BL-2024-004', DATEADD(day, -6, GETDATE()), 'bordereaux/BL-2024-004.pdf'),
(5, 'BL-2024-005', DATEADD(day, -19, GETDATE()), 'bordereaux/BL-2024-005.pdf');

-- 13. FACTURES
INSERT INTO Factures (idCommande, numero_facture, path_facture, created_at) VALUES
(1, 'FAC-2024-001', 'factures/FAC-2024-001.pdf', DATEADD(day, -14, GETDATE())),
(2, 'FAC-2024-002', 'factures/FAC-2024-002.pdf', DATEADD(day, -9, GETDATE())),
(3, 'FAC-2024-003', 'factures/FAC-2024-003.pdf', DATEADD(day, -3, GETDATE())),
(4, 'FAC-2024-004', 'factures/FAC-2024-004.pdf', DATEADD(day, -6, GETDATE())),
(5, 'FAC-2024-005', 'factures/FAC-2024-005.pdf', DATEADD(day, -19, GETDATE()));

-- 14. AVIS
INSERT INTO Avis (idClient, idProduit, note, commentaire, created_at, updated_at) VALUES
(5, 1, 5, 'Excellente huile d''argan ! Très efficace pour les cheveux et la peau.', DATEADD(day, -8, GETDATE()), DATEADD(day, -8, GETDATE())),
(5, 2, 4, 'Bon savon noir, texture agréable. Je recommande.', DATEADD(day, -8, GETDATE()), DATEADD(day, -8, GETDATE())),
(6, 7, 5, 'Miel de thym authentique, goût exceptionnel !', DATEADD(day, -5, GETDATE()), DATEADD(day, -5, GETDATE())),
(6, 9, 4, 'Pollen de bonne qualité, bon rapport qualité-prix.', DATEADD(day, -5, GETDATE()), DATEADD(day, -5, GETDATE())),
(8, 1, 5, 'Produit naturel et de très bonne qualité. Livraison rapide.', DATEADD(day, -2, GETDATE()), DATEADD(day, -2, GETDATE())),
(8, 4, 4, 'Huile d''olive savoureuse, parfaite pour la cuisine.', DATEADD(day, -2, GETDATE()), DATEADD(day, -2, GETDATE()));

-- ============================================
-- AFFICHER LES COMPTES DE TEST
-- ============================================

SELECT '========================================' AS Separator;
SELECT 'COMPTES DE TEST CRÉÉS' AS Message;
SELECT '========================================' AS Separator;
SELECT ' ' AS Blank;

SELECT 'SUPER ADMIN:' AS Type, email AS Email, 'super123' AS MotDePasse FROM Utilisateurs WHERE id = 4
UNION ALL
SELECT 'ADMIN 1:', email, 'admin123' FROM Utilisateurs WHERE id = 1
UNION ALL
SELECT 'ADMIN 2:', email, 'admin123' FROM Utilisateurs WHERE id = 2
UNION ALL
SELECT 'ADMIN 3:', email, 'admin123' FROM Utilisateurs WHERE id = 3
UNION ALL
SELECT 'CLIENT 1:', email, 'client123' FROM Utilisateurs WHERE id = 5
UNION ALL
SELECT 'CLIENT 2:', email, 'client123' FROM Utilisateurs WHERE id = 6
UNION ALL
SELECT 'CLIENT 3:', email, 'client123' FROM Utilisateurs WHERE id = 7
UNION ALL
SELECT 'CLIENT 4:', email, 'client123' FROM Utilisateurs WHERE id = 8;

SELECT ' ' AS Blank;
SELECT 'Base de données remplie avec succès !' AS Message;