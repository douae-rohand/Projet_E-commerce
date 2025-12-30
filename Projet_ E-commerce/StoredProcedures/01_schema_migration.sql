USE [cooperative];
GO

-- 1. Mettre à jour les status 'valide' -> 'livre' (comme demandé pâr l'utilisateur, bien que ce soit des commandes terminées)
-- Note: Si vous préférez que les anciennes commandes valides soient 'en_preparation', changez 'livre' par 'en_preparation'
UPDATE Commandes
SET statut = 'livre'
WHERE statut = 'valide';
GO

-- 2. Supprimer l'ancienne contrainte
IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CHK_Commandes_statut')
BEGIN
    ALTER TABLE Commandes
    DROP CONSTRAINT CHK_Commandes_statut;
END
GO

-- 3. Ajouter la nouvelle contrainte
ALTER TABLE Commandes
ADD CONSTRAINT CHK_Commandes_statut 
CHECK (statut IN ('annule', 'livre', 'en_livraison', 'en_preparation', 'en_attente'));
GO

-- 4. Supprimer la colonne statut de la table Livraisons (et sa contrainte associée)
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
