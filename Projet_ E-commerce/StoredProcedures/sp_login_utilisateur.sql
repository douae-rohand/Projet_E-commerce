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
