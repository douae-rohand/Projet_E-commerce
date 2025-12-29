-- Migration: Update Order and Delivery Flow Constraints
USE [cooperative];
GO

-- 1. Update Commandes Table Statuses
-- We need to find and drop the existing constraint first because we can't alter it directly.
DECLARE @ConstraintName NVARCHAR(255)
SELECT @ConstraintName = name 
FROM sys.check_constraints 
WHERE parent_object_id = OBJECT_ID('Commandes') AND definition LIKE '%statut%';

IF @ConstraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE Commandes DROP CONSTRAINT ' + @ConstraintName);
END
GO

-- Add the new robust constraint for Commandes
ALTER TABLE Commandes
ADD CONSTRAINT CK_Commandes_Statut 
CHECK (statut IN ('en_attente', 'valide', 'en_preparation', 'en_livraison', 'livre', 'annule'));
GO

-- 2. Update Livraisons Table Statuses and Fix Typo
-- Note: Table name is Livraisons (plural)
DECLARE @ConstraintNameLivraisonStatus NVARCHAR(255)
SELECT @ConstraintNameLivraisonStatus = name 
FROM sys.check_constraints 
WHERE parent_object_id = OBJECT_ID('Livraisons') AND definition LIKE '%statut%' AND definition LIKE '%livre%';

IF @ConstraintNameLivraisonStatus IS NOT NULL
BEGIN
    EXEC('ALTER TABLE Livraisons DROP CONSTRAINT ' + @ConstraintNameLivraisonStatus);
END
GO

-- Add new statut constraint for Livraisons
ALTER TABLE Livraisons
ADD CONSTRAINT CK_Livraisons_Statut 
CHECK (statut IN ('en_preparation', 'en_cours', 'livre', 'non_livre'));
GO

-- 3. Fix Mode_Livraison Typo (It was checking 'statut' column instead of 'mode_livraison')
-- Drop the erroneous constraint
DECLARE @ConstraintNameLivraisonMode NVARCHAR(255)
SELECT @ConstraintNameLivraisonMode = name 
FROM sys.check_constraints 
WHERE parent_object_id = OBJECT_ID('Livraisons') AND definition LIKE '%Standard%';

IF @ConstraintNameLivraisonMode IS NOT NULL
BEGIN
    EXEC('ALTER TABLE Livraisons DROP CONSTRAINT ' + @ConstraintNameLivraisonMode);
END
GO

-- Add correct constraint for mode_livraison
ALTER TABLE Livraisons
ADD CONSTRAINT CK_Livraisons_Mode
CHECK (mode_livraison IN ('Standard', 'Express'));
GO

PRINT 'Database Schema Updated Successfully (Livraisons fixed).';
