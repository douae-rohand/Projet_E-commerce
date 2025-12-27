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
