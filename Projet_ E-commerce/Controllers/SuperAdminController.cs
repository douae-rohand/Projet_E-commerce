using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projet__E_commerce.Models.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;
using Projet__E_commerce.Filters;

namespace Projet__E_commerce.Controllers
{
    [AuthorizeRole("SUPER_ADMIN")]
    public class SuperAdminController : Controller
    {
        private readonly ILogger<SuperAdminController> _logger;
        private readonly IConfiguration _configuration;

        public SuperAdminController(ILogger<SuperAdminController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            ViewBag.ActiveTab = "dashboard";

            var model = await GetCompleteDashboardDataAsync();
            model.UserEmail = HttpContext.Session.GetString("UserEmail");
            return View("~/Views/SuperAdmin/Dashboard.cshtml", model);
        }

        public async Task<IActionResult> Cooperatives()
        {
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            ViewBag.ActiveTab = "cooperatives";

            var cooperatives = await GetCooperativesAsync();
            return View("~/Views/SuperAdmin/Dashboard.cshtml", cooperatives);
        }

        public async Task<IActionResult> Users()
        {
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            ViewBag.ActiveTab = "users";

            var users = await GetUsersAsync();
            return View("~/Views/SuperAdmin/Dashboard.cshtml", users);
        }


        public async Task<IActionResult> DeliveriesAndOrders()
        {
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            ViewBag.ActiveTab = "deliveries_orders";

            var data = await GetDeliveriesAndOrdersAsync();
            return View("~/Views/SuperAdmin/Dashboard.cshtml", data);
        }

        public async Task<IActionResult> CooperativeDetails(int id)
        {
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");

            var cooperative = await GetCooperativeDetailsAsync(id);
            if (cooperative == null)
            {
                TempData["ErrorMessage"] = "Coopérative introuvable.";
                return RedirectToAction("Cooperatives");
            }

            return View("~/Views/SuperAdmin/CooperativeDetails.cshtml", cooperative);
        }

        private async Task<SuperAdminDashboardViewModel> GetDashboardDataAsync()
        {
            var model = new SuperAdminDashboardViewModel();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Get total cooperatives
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM admins", connection))
                {
                    model.TotalCooperatives = (int)await cmd.ExecuteScalarAsync();
                }

                // Get total users (clients)
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM clients", connection))
                {
                    model.TotalUsers = (int)await cmd.ExecuteScalarAsync();
                }

                // Get total orders
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM commandes WHERE statut NOT IN ('en_attente', 'annule')", connection))
                {
                    model.TotalOrders = (int)await cmd.ExecuteScalarAsync();
                }

                // Get total revenue
                using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(SUM(prixTotal), 0) FROM commandes WHERE statut NOT IN ('en_attente', 'annule')", connection))
                {
                    model.TotalRevenue = (decimal)await cmd.ExecuteScalarAsync();
                }

                // Get top cooperatives
                string topCoopsQuery = @"
                    SELECT TOP 5 
                        a.id, 
                        a.nom_cooperative, 
                        a.ville,
                        a.logo,
                        a.created_at,
                        COUNT(DISTINCT p.idP) as TotalProduits,
                        COUNT(DISTINCT CASE WHEN c.idCommande IS NOT NULL THEN c.idCommande ELSE NULL END) as TotalCommandes,
                        ISNULL(SUM(CASE WHEN c.idCommande IS NOT NULL THEN lc.prix_unitaire * lc.quantite ELSE 0 END), 0) as Revenue
                    FROM admins a
                    LEFT JOIN produits p ON a.id = p.idAdmin
                    LEFT JOIN variantes v ON p.idP = v.idP
                    LEFT JOIN lignescommande lc ON v.idV = lc.idV
                    LEFT JOIN commandes c ON lc.idCommande = c.idCommande AND c.statut NOT IN ('en_attente', 'annule')
                    GROUP BY a.id, a.nom_cooperative, a.ville, a.logo, a.created_at
                    ORDER BY Revenue DESC";

                using (SqlCommand cmd = new SqlCommand(topCoopsQuery, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        model.TopCooperatives.Add(new CooperativeStatsViewModel
                        {
                            Id = reader.GetInt32(0),
                            NomCooperative = reader.GetString(1),
                            Ville = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Logo = reader.IsDBNull(3) ? null : reader.GetString(3),
                            CreatedAt = reader.GetDateTime(4),
                            TotalProduits = reader.GetInt32(5),
                            TotalCommandes = reader.GetInt32(6),
                            Revenue = reader.GetDecimal(7)
                        });
                    }
                }

                // Get recent activities
                string activitiesQuery = @"
                    SELECT TOP 10 
                        'order' as Type,
                        'Nouvelle commande' as Title,
                        CONCAT('Commande #', c.idCommande, ' - ', cl.prenom, ' ', cl.nom) as Detail,
                        c.created_at
                    FROM commandes c
                    INNER JOIN clients cl ON c.idClient = cl.id
                    ORDER BY c.created_at DESC";

                using (SqlCommand cmd = new SqlCommand(activitiesQuery, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var createdAt = reader.GetDateTime(3);
                        var timeAgo = GetTimeAgo(createdAt);

                        model.RecentActivities.Add(new RecentActivityViewModel
                        {
                            Type = reader.GetString(0),
                            Title = reader.GetString(1),
                            Detail = reader.GetString(2),
                            Time = timeAgo,
                            Icon = "bi-box",
                            Color = "success"
                        });
                    }
                }
            }

            return model;
        }

        private async Task<List<CooperativeStatsViewModel>> GetCooperativesAsync()
        {
            var cooperatives = new List<CooperativeStatsViewModel>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                    SELECT 
                        a.id, 
                        a.nom_cooperative, 
                        a.ville,
                        a.logo,
                        a.telephone,
                        a.created_at,
                        COUNT(DISTINCT p.idP) as TotalProduits,
                        COUNT(DISTINCT c.idCommande) as TotalCommandes,
                        ISNULL(SUM(CASE WHEN c.idCommande IS NOT NULL THEN lc.prix_unitaire * lc.quantite ELSE 0 END), 0) as Revenue
                    FROM admins a
                    LEFT JOIN produits p ON a.id = p.idAdmin
                    LEFT JOIN variantes v ON p.idP = v.idP
                    LEFT JOIN lignescommande lc ON v.idV = lc.idV
                    LEFT JOIN commandes c ON lc.idCommande = c.idCommande AND c.statut NOT IN ('en_attente', 'annule')
                    GROUP BY a.id, a.nom_cooperative, a.ville, a.logo, a.telephone, a.created_at
                    ORDER BY a.created_at DESC";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        cooperatives.Add(new CooperativeStatsViewModel
                        {
                            Id = reader.GetInt32(0),
                            NomCooperative = reader.GetString(1),
                            Ville = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Logo = reader.IsDBNull(3) ? null : reader.GetString(3),
                            CreatedAt = reader.GetDateTime(5),
                            TotalProduits = reader.GetInt32(6),
                            TotalCommandes = reader.GetInt32(7),
                            Revenue = reader.GetDecimal(8)
                        });
                    }
                }
            }

            return cooperatives;
        }

        private async Task<List<UserStatsViewModel>> GetUsersAsync()
        {
            var users = new List<UserStatsViewModel>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Get Clients
                string clientsQuery = @"
                    SELECT 
                        u.id, 
                        u.email, 
                        'CLIENT' as Role,
                        c.nom,
                        c.prenom,
                        NULL as NomCooperative,
                        u.est_actif,
                        u.created_at
                    FROM utilisateurs u
                    INNER JOIN clients c ON u.id = c.id";

                using (SqlCommand cmd = new SqlCommand(clientsQuery, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new UserStatsViewModel
                        {
                            Id = reader.GetInt32(0),
                            Email = reader.GetString(1),
                            Role = reader.GetString(2),
                            Nom = reader.GetString(3),
                            Prenom = reader.GetString(4),
                            NomCooperative = null,
                            EstActif = reader.GetBoolean(6),
                            CreatedAt = reader.GetDateTime(7)
                        });
                    }
                }

                // Get Admins
                string adminsQuery = @"
                    SELECT 
                        u.id, 
                        u.email, 
                        'ADMIN' as Role,
                        NULL as Nom,
                        NULL as Prenom,
                        a.nom_cooperative,
                        u.est_actif,
                        u.created_at
                    FROM utilisateurs u
                    INNER JOIN admins a ON u.id = a.id";

                using (SqlCommand cmd = new SqlCommand(adminsQuery, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new UserStatsViewModel
                        {
                            Id = reader.GetInt32(0),
                            Email = reader.GetString(1),
                            Role = reader.GetString(2),
                            Nom = null,
                            Prenom = null,
                            NomCooperative = reader.GetString(5),
                            EstActif = reader.GetBoolean(6),
                            CreatedAt = reader.GetDateTime(7)
                        });
                    }
                }
            }

            return users.OrderByDescending(u => u.CreatedAt).ToList();
        }


        private async Task<List<OrderDetailsViewModel>> GetDeliveriesAndOrdersAsync()
        {
            var list = new List<OrderDetailsViewModel>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                    SELECT 
                        c.idCommande, c.created_at, c.statut, c.prixTotal,
                        cl.nom, cl.prenom, u.email, cl.telephone,
                        al.adresse_complete, al.ville, al.code_postal, 
                        l.mode_livraison
                    FROM Commandes c
                    INNER JOIN Clients cl ON c.idClient = cl.id
                    INNER JOIN Utilisateurs u ON cl.id = u.id
                    LEFT JOIN Livraisons l ON c.idCommande = l.idCommande
                    LEFT JOIN AdressesLivraison al ON l.idAdresse = al.idAdresse
                    WHERE c.statut != 'en_attente'
                    ORDER BY c.created_at DESC";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new OrderDetailsViewModel
                        {
                            IdCommande = reader.GetInt32(0),
                            DateCommande = reader.GetDateTime(1),
                            Statut = reader.GetString(2),
                            MontantTotal = reader.GetDecimal(3),
                            ClientNom = reader.GetString(4),
                            ClientPrenom = reader.GetString(5),
                            ClientEmail = reader.GetString(6),
                            ClientTelephone = reader.IsDBNull(7) ? "" : reader.GetString(7),
                            AdresseLivraison = reader.IsDBNull(8) ? "" : reader.GetString(8),
                            VilleLivraison = reader.IsDBNull(9) ? "" : reader.GetString(9),
                            CodePostalLivraison = reader.IsDBNull(10) ? "" : reader.GetString(10),
                            HasDelivery = !reader.IsDBNull(11),
                            //DeliveryStatus = reader.IsDBNull(11) ? null : reader.GetString(11),
                            DeliveryMode = reader.IsDBNull(11) ? null : reader.GetString(11)
                        });
                    }
                }
            }

            return list;
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetailsModal(int id)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                OrderDetailsViewModel? order = null;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"
                        SELECT 
                            c.idCommande, c.created_at, c.statut, c.prixTotal,
                            cl.nom, cl.prenom, u.email, cl.telephone,
                            al.adresse_complete, al.ville, al.code_postal, 
                            l.mode_livraison, l.frais as fraisLivraison,
                            l.notes,
                            f.numero_facture, f.path_facture,
                            bl.numero_bordereau, bl.path_bordereau
                        FROM Commandes c
                        LEFT JOIN Clients cl ON c.idClient = cl.id
                        LEFT JOIN Utilisateurs u ON cl.id = u.id
                        LEFT JOIN Livraisons l ON c.idCommande = l.idCommande
                        LEFT JOIN AdressesLivraison al ON l.idAdresse = al.idAdresse
                        LEFT JOIN Factures f ON c.idCommande = f.idCommande
                        LEFT JOIN BordereauxLivraison bl ON l.idLivraison = bl.idLivraison
                        WHERE c.idCommande = @id";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                order = new OrderDetailsViewModel
                                {
                                    IdCommande = Convert.ToInt32(reader["idCommande"]),
                                    DateCommande = Convert.ToDateTime(reader["created_at"]),
                                    Statut = reader["statut"]?.ToString() ?? "N/A",
                                    MontantTotal = Convert.ToDecimal(reader["prixTotal"]),
                                    ClientNom = reader["nom"]?.ToString() ?? "N/A",
                                    ClientPrenom = reader["prenom"]?.ToString() ?? "N/A",
                                    ClientEmail = reader["email"]?.ToString() ?? "N/A",
                                    ClientTelephone = reader["telephone"]?.ToString() ?? "N/A",
                                    AdresseLivraison = reader["adresse_complete"]?.ToString() ?? "N/A",
                                    VilleLivraison = reader["ville"]?.ToString() ?? "N/A",
                                    CodePostalLivraison = reader["code_postal"]?.ToString() ?? "N/A",
                                    HasDelivery = reader["mode_livraison"] != DBNull.Value,
                                    DeliveryStatus = reader["mode_livraison"] != DBNull.Value ? "Pris en charge" : null,
                                    DeliveryMode = reader["mode_livraison"] == DBNull.Value ? null : reader["mode_livraison"].ToString(),
                                    FraisLivraison = reader["fraisLivraison"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["fraisLivraison"]),
                                    Notes = reader["notes"] == DBNull.Value ? null : reader["notes"].ToString(),
                                    NumeroFacture = reader["numero_facture"] == DBNull.Value ? null : reader["numero_facture"].ToString(),
                                    PathFacture = reader["path_facture"] == DBNull.Value ? null : (reader["path_facture"].ToString()?.StartsWith("/") == true ? reader["path_facture"].ToString() : "/factures/" + reader["path_facture"].ToString()),
                                    NumeroBordereau = reader["numero_bordereau"] == DBNull.Value ? null : reader["numero_bordereau"].ToString(),
                                    PathBordereau = reader["path_bordereau"] == DBNull.Value ? null : (reader["path_bordereau"].ToString()?.StartsWith("/") == true ? reader["path_bordereau"].ToString() : "/bordereaux/" + reader["path_bordereau"].ToString())
                                };
                            }
                        }
                    }

                    if (order != null)
                    {
                        string itemsQuery = @"
                            SELECT lc.idLC, p.nomP, p.description, a.nom_cooperative, lc.quantite, lc.prix_unitaire, (lc.quantite * lc.prix_unitaire) as sousTotal, v.photo, v.taille, v.couleur, v.poids
                            FROM LignesCommande lc
                            LEFT JOIN Variantes v ON lc.idV = v.idV
                            LEFT JOIN Produits p ON v.idP = p.idP
                            LEFT JOIN Admins a ON p.idAdmin = a.id
                            WHERE lc.idCommande = @orderId";

                        using (SqlCommand itemsCmd = new SqlCommand(itemsQuery, connection))
                        {
                            itemsCmd.Parameters.AddWithValue("@orderId", order.IdCommande);
                            using (SqlDataReader itemsReader = await itemsCmd.ExecuteReaderAsync())
                            {
                                while (await itemsReader.ReadAsync())
                                {
                                    order.Items.Add(new OrderItemViewModel
                                    {
                                        IdProduit = itemsReader["idLC"] != DBNull.Value ? Convert.ToInt32(itemsReader["idLC"]) : 0,
                                        NomProduit = itemsReader["nomP"]?.ToString() ?? "Produit inconnu",
                                        NomCooperative = itemsReader["nom_cooperative"]?.ToString() ?? "N/A",
                                        Quantite = itemsReader["quantite"] != DBNull.Value ? Convert.ToInt32(itemsReader["quantite"]) : 0,
                                        PrixUnitaire = itemsReader["prix_unitaire"] != DBNull.Value ? Convert.ToDecimal(itemsReader["prix_unitaire"]) : 0,
                                        SousTotal = itemsReader["sousTotal"] != DBNull.Value ? Convert.ToDecimal(itemsReader["sousTotal"]) : 0,
                                        ImageUrl = itemsReader["photo"] == DBNull.Value ? null : itemsReader["photo"].ToString(),
                                        Description = itemsReader["description"] == DBNull.Value ? null : itemsReader["description"].ToString(),
                                        Taille = itemsReader["taille"] == DBNull.Value ? null : itemsReader["taille"].ToString(),
                                        Couleur = itemsReader["couleur"] == DBNull.Value ? null : itemsReader["couleur"].ToString(),
                                        Poids = itemsReader["poids"] == DBNull.Value ? null : itemsReader["poids"].ToString()
                                    });
                                }
                            }
                        }
                    }
                }

                if (order == null) return Content("<div class='alert alert-info'>Commande introuvable ou aucune donnée associée.</div>");

                return PartialView("~/Views/SuperAdmin/_OrderModalContent.cshtml", order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des détails de la commande");
                return Content($"<div class='alert alert-danger'>Une erreur serveur est survenue : {ex.Message}</div>");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDeliveryStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                if (request == null) return Json(new { success = false, message = "Données invalides." });

                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Livraison status is now tied to Commande status. 
                    // Let's update the Commande status instead if this is called.
                    string updateQuery = "UPDATE Commandes SET statut = @status, updated_at = @updatedAt WHERE idCommande = @idCommande";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@status", request.NewStatus);
                        cmd.Parameters.AddWithValue("@updatedAt", DateTime.Now);
                        cmd.Parameters.AddWithValue("@idCommande", request.IdCommande);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Aucune livraison trouvée pour cette commande." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du statut de livraison");
                return Json(new { success = false, message = "Une erreur est survenue lors de la mise à jour : " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] StatusUpdateRequest request)
        {
            try
            {
                if (request == null) return Json(new { success = false, message = "Données invalides." });

                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string updateQuery = "UPDATE Commandes SET statut = @status, updated_at = @updatedAt WHERE idCommande = @idCommande";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@status", request.NewStatus);
                        cmd.Parameters.AddWithValue("@updatedAt", DateTime.Now);
                        cmd.Parameters.AddWithValue("@idCommande", request.IdCommande);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Aucune commande trouvée." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du statut de commande");
                return Json(new { success = false, message = "Une erreur est survenue lors de la mise à jour : " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> UserDetailsModal(int id)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                UserDetailsViewModel? user = null;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Get user basic info
                    string userQuery = @"
                        SELECT 
                            u.id, u.email, u.est_actif, u.created_at,
                            CASE 
                                WHEN c.id IS NOT NULL THEN 'CLIENT'
                                WHEN a.id IS NOT NULL THEN 'ADMIN'
                                ELSE 'UNKNOWN'
                            END as Role,
                            c.nom, c.prenom, c.telephone,
                            a.nom_cooperative, a.localisation, a.ville
                        FROM utilisateurs u
                        LEFT JOIN clients c ON u.id = c.id
                        LEFT JOIN admins a ON u.id = a.id
                        WHERE u.id = @id";

                    using (SqlCommand cmd = new SqlCommand(userQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                user = new UserDetailsViewModel
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Email = reader["email"]?.ToString() ?? "N/A",
                                    Role = reader["Role"]?.ToString() ?? "UNKNOWN",
                                    Nom = reader["nom"]?.ToString(),
                                    Prenom = reader["prenom"]?.ToString(),
                                    NomCooperative = reader["nom_cooperative"]?.ToString(),
                                    EstActif = reader["est_actif"] != DBNull.Value && Convert.ToBoolean(reader["est_actif"]),
                                    CreatedAt = reader["created_at"] != DBNull.Value ? Convert.ToDateTime(reader["created_at"]) : DateTime.MinValue,
                                    Telephone = reader["telephone"]?.ToString(),
                                    Adresse = reader["localisation"]?.ToString(), // For admins, use localisation as address
                                    Ville = reader["ville"]?.ToString(),
                                    CodePostal = null // Not available in current schema
                                };
                            }
                        }
                    }

                    if (user != null)
                    {
                        // Get order statistics
                        string statsQuery = @"
                            SELECT 
                                COUNT(*) as totalOrders,
                                COALESCE(SUM(prixTotal), 0) as totalSpent,
                                MAX(created_at) as lastOrderDate
                            FROM Commandes 
                            WHERE idClient = @userId AND statut NOT IN ('en_attente', 'annule')";

                        using (SqlCommand statsCmd = new SqlCommand(statsQuery, connection))
                        {
                            statsCmd.Parameters.AddWithValue("@userId", user.Id);
                            using (SqlDataReader statsReader = await statsCmd.ExecuteReaderAsync())
                            {
                                if (await statsReader.ReadAsync())
                                {
                                    user.TotalOrders = statsReader["totalOrders"] != DBNull.Value ? Convert.ToInt32(statsReader["totalOrders"]) : 0;
                                    user.TotalSpent = statsReader["totalSpent"] != DBNull.Value ? Convert.ToDecimal(statsReader["totalSpent"]) : 0;
                                    user.LastOrderDate = statsReader["lastOrderDate"] != DBNull.Value ? Convert.ToDateTime(statsReader["lastOrderDate"]) : (DateTime?)null;
                                }
                            }
                        }

                        // Get recent orders
                        string ordersQuery = @"
                            SELECT TOP 5 idCommande, created_at, statut, prixTotal
                            FROM Commandes 
                            WHERE idClient = @userId
                            ORDER BY created_at DESC";

                        using (SqlCommand ordersCmd = new SqlCommand(ordersQuery, connection))
                        {
                            ordersCmd.Parameters.AddWithValue("@userId", user.Id);
                            using (SqlDataReader ordersReader = await ordersCmd.ExecuteReaderAsync())
                            {
                                while (await ordersReader.ReadAsync())
                                {
                                    user.RecentOrders.Add(new OrderSummaryViewModel
                                    {
                                        IdCommande = Convert.ToInt32(ordersReader["idCommande"]),
                                        DateCommande = Convert.ToDateTime(ordersReader["created_at"]),
                                        Statut = ordersReader["statut"]?.ToString() ?? "N/A",
                                        MontantTotal = ordersReader["prixTotal"] != DBNull.Value ? Convert.ToDecimal(ordersReader["prixTotal"]) : 0
                                    });
                                }
                            }
                        }
                    }
                }

                if (user == null) return Content("<div class='alert alert-info'>Utilisateur introuvable.</div>");

                return PartialView("~/Views/SuperAdmin/_UserModalContent.cshtml", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des détails de l'utilisateur");
                return Content($"<div class='alert alert-danger'>Une erreur serveur est survenue : {ex.Message}</div>");
            }
        }

        private async Task<CooperativeDetailsViewModel?> GetCooperativeDetailsAsync(int id)
        {
            var model = new CooperativeDetailsViewModel();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Get cooperative basic info
                string coopQuery = @"
                    SELECT 
                        a.id, a.nom_cooperative, u.email, a.ville, a.localisation, 
                        a.telephone, a.logo, a.description, u.est_actif, a.created_at
                    FROM admins a
                    INNER JOIN utilisateurs u ON a.id = u.id
                    WHERE a.id = @Id";

                using (SqlCommand cmd = new SqlCommand(coopQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            model.Id = reader.GetInt32(0);
                            model.NomCooperative = reader.GetString(1);
                            model.Email = reader.GetString(2);
                            model.Ville = reader.IsDBNull(3) ? null : reader.GetString(3);
                            model.Localisation = reader.IsDBNull(4) ? null : reader.GetString(4);
                            model.Telephone = reader.IsDBNull(5) ? null : reader.GetString(5);
                            model.Logo = reader.IsDBNull(6) ? null : reader.GetString(6);
                            model.Description = reader.IsDBNull(7) ? null : reader.GetString(7);
                            model.EstActif = reader.GetBoolean(8);
                            model.CreatedAt = reader.GetDateTime(9);
                        }
                        else
                        {
                            return null; // Cooperative not found
                        }
                    }
                }

                // Get statistics
                string statsQuery = @"
                    SELECT 
                        COUNT(DISTINCT p.idP) as TotalProduits,
                        COUNT(DISTINCT c.idCommande) as TotalCommandes,
                        ISNULL(SUM(CASE WHEN c.idCommande IS NOT NULL THEN lc.prix_unitaire * lc.quantite ELSE 0 END), 0) as Revenue
                    FROM admins a
                    LEFT JOIN produits p ON a.id = p.idAdmin
                    LEFT JOIN variantes v ON p.idP = v.idP
                    LEFT JOIN lignescommande lc ON v.idV = lc.idV
                    LEFT JOIN commandes c ON lc.idCommande = c.idCommande AND c.statut NOT IN ('en_attente', 'annule')
                    WHERE a.id = @Id";

                using (SqlCommand cmd = new SqlCommand(statsQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            model.TotalProduits = reader.GetInt32(0);
                            model.TotalCommandes = reader.GetInt32(1);
                            model.Revenue = reader.GetDecimal(2);
                        }
                    }
                }

                // Get products with details
                string productsQuery = @"
                    SELECT 
                        p.idP, p.nomP, p.description, MIN(v.prix) as prix, 
                        ISNULL(SUM(v.quantite), 0) as Stock,
                        (SELECT TOP 1 v2.photo FROM variantes v2 WHERE v2.idP = p.idP) as image_url, 
                        c.nom as Categorie,
                        (SELECT AVG(CAST(note as FLOAT)) FROM avis WHERE idProduit = p.idP) as MoyenneAvis,
                        (SELECT COUNT(*) FROM avis WHERE idProduit = p.idP) as NombreAvis
                    FROM produits p
                    LEFT JOIN variantes v ON p.idP = v.idP
                    LEFT JOIN categories c ON p.idC = c.idC
                    WHERE p.idAdmin = @Id
                    GROUP BY p.idP, p.nomP, p.description, c.nom";

                using (SqlCommand cmd = new SqlCommand(productsQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var product = new ProductViewModel
                            {
                                IdP = reader.GetInt32(0),
                                NomP = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Prix = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                                Stock = reader.GetInt32(4),
                                ImageUrl = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Categorie = reader.GetString(6),
                                MoyenneAvis = reader.IsDBNull(7) ? null : (double?)reader.GetDouble(7),
                                NombreAvis = reader.GetInt32(8)
                            };

                            // Get reviews for this product
                            string avisQuery = @"
                                SELECT a.idAvis, cl.prenom, cl.nom, a.note, a.commentaire, a.created_at
                                FROM avis a
                                INNER JOIN clients cl ON a.idClient = cl.id
                                WHERE a.idProduit = @ProductId
                                ORDER BY a.created_at DESC";

                            using (SqlCommand avisCmd = new SqlCommand(avisQuery, connection))
                            {
                                avisCmd.Parameters.AddWithValue("@ProductId", product.IdP);
                                using (SqlDataReader avisReader = await avisCmd.ExecuteReaderAsync())
                                {
                                    while (await avisReader.ReadAsync())
                                    {
                                        product.Avis.Add(new AvisViewModel
                                        {
                                            IdAvis = avisReader.GetInt32(0),
                                            ClientPrenom = avisReader.GetString(1),
                                            ClientNom = avisReader.GetString(2),
                                            Note = avisReader.GetInt32(3),
                                            Commentaire = avisReader.IsDBNull(4) ? null : avisReader.GetString(4),
                                            DateAvis = avisReader.GetDateTime(5)
                                        });
                                    }
                                }
                            }

                            model.Produits.Add(product);
                        }
                    }
                }
            }

            return model;
        }

        private async Task<SuperAdminDashboardViewModel> GetCompleteDashboardDataAsync()
        {
            var model = new SuperAdminDashboardViewModel();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // ===== STATISTIQUES GÉNÉRALES =====
                
                // Total cooperatives
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        COUNT(*) as Total,
                        COUNT(CASE WHEN u.est_actif = 1 THEN 1 END) as Actives
                    FROM admins a 
                    INNER JOIN utilisateurs u ON a.id = u.id", connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        model.TotalCooperatives = reader.GetInt32(0);
                        model.TotalCooperativesActives = reader.GetInt32(1);
                    }
                }

                // Total users and clients
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        COUNT(*) as TotalUsers,
                        ISNULL(SUM(CASE WHEN est_actif = 1 THEN 1 ELSE 0 END), 0) as UsersActifs,
                        (SELECT COUNT(*) FROM clients) as TotalClients
                    FROM utilisateurs", connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        model.TotalUsers = reader.GetInt32(0);
                        model.TotalUsersActifs = reader.GetInt32(1);
                        model.TotalClients = reader.GetInt32(2);
                    }
                }

                // Total produits and stock statistics
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        COUNT(*) as TotalProduits,
                        ISNULL(SUM(CASE WHEN v.quantite > 0 THEN 1 ELSE 0 END), 0) as ProduitsEnStock,
                        ISNULL(SUM(CASE WHEN v.quantite <= p.seuil_alerte AND v.quantite > 0 THEN 1 ELSE 0 END), 0) as ProduitsStockFaible,
                        ISNULL(SUM(CASE WHEN v.quantite = 0 THEN 1 ELSE 0 END), 0) as ProduitsRupture
                    FROM Produits p
                    LEFT JOIN Variantes v ON p.idP = v.idP", connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        model.TotalProduits = reader.GetInt32(0);
                        model.ProduitsEnStock = reader.GetInt32(1);
                        model.ProduitsStockFaible = reader.GetInt32(2);
                        model.ProduitsRupture = reader.GetInt32(3);
                        // NEW: derive "stock OK" count for the dashboard
                        model.ProduitsStockOk = Math.Max(0, model.ProduitsEnStock - model.ProduitsStockFaible);
                    }
                }

                // Total commandes et revenus
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        COUNT(*) as TotalCommandes,
                        ISNULL(SUM(prixTotal), 0) as TotalRevenue
                    FROM Commandes
                    WHERE statut NOT IN ('en_attente', 'annule')", connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        model.TotalOrders = reader.GetInt32(0);
                        model.TotalRevenue = reader.GetDecimal(1);
                        model.AverageOrderValue = model.TotalOrders > 0 ? model.TotalRevenue / model.TotalOrders : 0;
                    }
                }

                // Top Cities by Revenue
                string topCitiesQuery = @"
                    SELECT TOP 5 
                        al.ville,
                        COUNT(c.idCommande) as OrderCount,
                        SUM(c.prixTotal) as Revenue
                    FROM Commandes c
                    INNER JOIN Livraisons l ON c.idCommande = l.idCommande
                    INNER JOIN AdressesLivraison al ON l.idAdresse = al.idAdresse
                    WHERE c.statut NOT IN ('en_attente', 'annule')
                    GROUP BY al.ville
                    ORDER BY Revenue DESC";

                using (SqlCommand cmd = new SqlCommand(topCitiesQuery, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        model.TopCities.Add(new CityRevenueViewModel
                        {
                            City = reader.IsDBNull(0) ? "Inconnue" : reader.GetString(0),
                            OrderCount = reader.GetInt32(1),
                            Revenue = reader.GetDecimal(2)
                        });
                    }
                }

                // ===== TOP COOPÉRATIVES =====
                string topCoopsQuery = @"
                    SELECT TOP 5 
                        a.id, 
                        a.nom_cooperative, 
                        a.ville,
                        a.logo,
                        a.created_at,
                        COUNT(DISTINCT p.idP) as TotalProduits,
                        COUNT(DISTINCT c.idCommande) as TotalCommandes,
                        ISNULL(SUM(CASE WHEN c.idCommande IS NOT NULL THEN lc.prix_unitaire * lc.quantite ELSE 0 END), 0) as Revenue
                    FROM Admins a
                    LEFT JOIN Produits p ON a.id = p.idAdmin
                    LEFT JOIN Variantes v ON p.idP = v.idP
                    LEFT JOIN LignesCommande lc ON v.idV = lc.idV
                    LEFT JOIN Commandes c ON lc.idCommande = c.idCommande AND c.statut NOT IN ('en_attente', 'annule')
                    GROUP BY a.id, a.nom_cooperative, a.ville, a.logo, a.created_at
                    ORDER BY Revenue DESC";

                using (SqlCommand cmd = new SqlCommand(topCoopsQuery, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        model.TopCooperatives.Add(new CooperativeStatsViewModel
                        {
                            Id = reader.GetInt32(0),
                            NomCooperative = reader.GetString(1),
                            Ville = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Logo = reader.IsDBNull(3) ? null : reader.GetString(3),
                            CreatedAt = reader.GetDateTime(4),
                            TotalProduits = reader.GetInt32(5),
                            TotalCommandes = reader.GetInt32(6),
                            Revenue = reader.GetDecimal(7)
                        });
                    }
                }

                // ===== TOP PRODUITS =====
                string topProduitsQuery = @"
                    SELECT TOP 5
                        p.idP, p.nomP, a.nom_cooperative, 
                        (SELECT TOP 1 v2.photo FROM variantes v2 WHERE v2.idP = p.idP) as image_url,
                        MIN(v.prix) as prix,
                        COUNT(DISTINCT c.idCommande) as NombreVentes,
                        ISNULL(SUM(CASE WHEN c.idCommande IS NOT NULL THEN lc.prix_unitaire * lc.quantite ELSE 0 END), 0) as RevenusGeneres,
                        (SELECT AVG(CAST(note as FLOAT)) FROM avis WHERE idProduit = p.idP) as MoyenneAvis
                    FROM produits p
                    INNER JOIN admins a ON p.idAdmin = a.id
                    LEFT JOIN variantes v ON p.idP = v.idP
                    LEFT JOIN lignescommande lc ON v.idV = lc.idV
                    LEFT JOIN commandes c ON lc.idCommande = c.idCommande AND c.statut NOT IN ('en_attente', 'annule')
                    WHERE v.idV IS NOT NULL
                    GROUP BY p.idP, p.nomP, a.nom_cooperative
                    ORDER BY NombreVentes DESC";

                using (SqlCommand cmd = new SqlCommand(topProduitsQuery, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        model.TopProduits.Add(new ProductPerformanceViewModel
                        {
                            IdProduit = reader.GetInt32(0),
                            NomProduit = reader.GetString(1),
                            NomCooperative = reader.GetString(2),
                            ImageUrl = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Prix = reader.GetDecimal(4),
                            NombreVentes = reader.GetInt32(5),
                            RevenusGeneres = reader.GetDecimal(6),
                            MoyenneAvis = reader.IsDBNull(7) ? null : (double?)reader.GetDouble(7)
                        });
                    }
                }

                // ===== COMMANDES RÉCENTES =====
                string recentOrdersQuery = @"
                    SELECT TOP 10
                        c.idCommande, c.created_at, c.statut, c.prixTotal,
                        cl.nom, cl.prenom, u.email, cl.telephone,
                        al.adresse_complete, al.ville, al.code_postal, l.notes
                    FROM commandes c
                    INNER JOIN clients cl ON c.idClient = cl.id
                    INNER JOIN utilisateurs u ON cl.id = u.id
                    LEFT JOIN livraisons l ON c.idCommande = l.idCommande
                    LEFT JOIN adresseslivraison al ON l.idAdresse = al.idAdresse
                    ORDER BY c.created_at DESC";

                using (SqlCommand cmd = new SqlCommand(recentOrdersQuery, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        model.RecentOrders.Add(new OrderDetailsViewModel
                        {
                            IdCommande = reader.GetInt32(0),
                            DateCommande = reader.GetDateTime(1),
                            Statut = reader.GetString(2),
                            MontantTotal = reader.GetDecimal(3),
                            ClientNom = reader.GetString(4),
                            ClientPrenom = reader.GetString(5),
                            ClientEmail = reader.GetString(6),
                            ClientTelephone = reader.IsDBNull(7) ? "" : reader.GetString(7),
                            AdresseLivraison = reader.IsDBNull(8) ? "" : reader.GetString(8),
                            VilleLivraison = reader.IsDBNull(9) ? "" : reader.GetString(9),
                            CodePostalLivraison = reader.IsDBNull(10) ? "" : reader.GetString(10),
                            Notes = reader.IsDBNull(11) ? null : reader.GetString(11)
                        });
                    }
                }

                // Populate items for each order sequentially
                foreach (var order in model.RecentOrders)
                {
                    string orderItemsQuery = @"
                        SELECT p.idP, p.nomP, a.nom_cooperative, lc.quantite, lc.prix_unitaire, (lc.quantite * lc.prix_unitaire) as sousTotal
                        FROM lignescommande lc
                        INNER JOIN variantes v ON lc.idV = v.idV
                        INNER JOIN produits p ON v.idP = p.idP
                        INNER JOIN admins a ON p.idAdmin = a.id
                        WHERE lc.idCommande = @orderId";

                    using (SqlCommand itemsCmd = new SqlCommand(orderItemsQuery, connection))
                    {
                        itemsCmd.Parameters.AddWithValue("@orderId", order.IdCommande);
                        using (SqlDataReader itemsReader = await itemsCmd.ExecuteReaderAsync())
                        {
                            while (await itemsReader.ReadAsync())
                            {
                                order.Items.Add(new OrderItemViewModel
                                {
                                    IdProduit = itemsReader.GetInt32(0),
                                    NomProduit = itemsReader.GetString(1),
                                    NomCooperative = itemsReader.GetString(2),
                                    Quantite = itemsReader.GetInt32(3),
                                    PrixUnitaire = itemsReader.GetDecimal(4),
                                    SousTotal = itemsReader.GetDecimal(5)
                                });
                            }
                        }
                    }
                }

                // ===== ANALYTICS DATA (Merged) =====
                
                // Monthly revenue
                string monthlyQuery = @"
                    SELECT 
                        FORMAT(created_at, 'yyyy-MM') as Month,
                        COUNT(*) as Orders,
                        ISNULL(SUM(prixTotal), 0) as Revenue
                    FROM commandes
                    WHERE created_at >= DATEADD(MONTH, -6, GETDATE())
                      AND statut NOT IN ('en_attente', 'annule')
                    GROUP BY FORMAT(created_at, 'yyyy-MM')
                    ORDER BY Month";

                using (SqlCommand cmd = new SqlCommand(monthlyQuery, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        model.MonthlyRevenue.Add(new MonthlyRevenueViewModel
                        {
                            Month = reader.GetString(0),
                            Orders = reader.GetInt32(1),
                            Revenue = reader.GetDecimal(2)
                        });
                    }
                }

                // Category sales
                string categoryQuery = @"
                    SELECT 
                        c.nom as CategoryName,
                        COUNT(DISTINCT p.idP) as TotalProducts,
                        COUNT(DISTINCT co.idCommande) as TotalSales,
                        ISNULL(SUM(CASE WHEN co.idCommande IS NOT NULL THEN lc.prix_unitaire * lc.quantite ELSE 0 END), 0) as Revenue
                    FROM categories c
                    LEFT JOIN produits p ON c.idC = p.idC
                    LEFT JOIN variantes v ON p.idP = v.idP
                    LEFT JOIN lignescommande lc ON v.idV = lc.idV
                    LEFT JOIN commandes co ON lc.idCommande = co.idCommande AND co.statut NOT IN ('en_attente', 'annule')
                    GROUP BY c.nom
                    ORDER BY Revenue DESC";

                using (SqlCommand cmd = new SqlCommand(categoryQuery, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        model.CategorySales.Add(new CategorySalesViewModel
                        {
                            CategoryName = reader.GetString(0),
                            TotalProducts = reader.GetInt32(1),
                            TotalSales = reader.GetInt32(2),
                            Revenue = reader.GetDecimal(3)
                        });
                    }
                }
            }

            return model;
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "à l'instant";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}min";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h";
            if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays}j";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)}mois";
            return $"{(int)(timeSpan.TotalDays / 365)}ans";
        }

        public class UserStatusUpdateModel
        {
            public int UserId { get; set; }
            public bool IsActive { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus([FromBody] UserStatusUpdateModel model)
        {
            if (model == null) return Json(new { success = false, message = "Données invalides." });

            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string updateQuery = "UPDATE Utilisateurs SET est_actif = @newStatus WHERE id = @userId";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", model.UserId);
                        cmd.Parameters.AddWithValue("@newStatus", model.IsActive);
                        int affectedRows = await cmd.ExecuteNonQueryAsync();
                        
                        if (affectedRows == 0)
                        {
                            return Json(new { success = false, message = "Utilisateur non trouvé." });
                        }
                    }
                }

                return Json(new { success = true, message = "Statut de l'utilisateur mis à jour avec succès." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du statut utilisateur");
                return Json(new { success = false, message = "Erreur lors de la mise à jour du statut." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ValidateOrder(int orderId)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string updateQuery = "UPDATE Commandes SET statut = 'valide' WHERE idCommande = @orderId";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@orderId", orderId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return Json(new { success = true, message = "Commande validée avec succès." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation de la commande");
                return Json(new { success = false, message = "Erreur lors de la validation de la commande." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string updateQuery = "UPDATE Commandes SET statut = 'annule' WHERE idCommande = @orderId";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@orderId", orderId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return Json(new { success = true, message = "Commande annulée avec succès." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'annulation de la commande");
                return Json(new { success = false, message = "Erreur lors de l'annulation de la commande." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateDelivery(int orderId)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Check if delivery already exists
                    string checkQuery = "SELECT COUNT(*) FROM Livraisons WHERE idCommande = @orderId";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@orderId", orderId);
                        int count = (int)await checkCmd.ExecuteScalarAsync();
                        if (count > 0)
                        {
                            return Json(new { success = false, message = "Une livraison existe déjà pour cette commande." });
                        }
                    }

                    // Get delivery address from order
                    string addressQuery = @"SELECT TOP 1 al.idAdresse 
                                           FROM Livraisons l 
                                           INNER JOIN AdressesLivraison al ON l.idAdresse = al.idAdresse 
                                           WHERE l.idCommande = @orderId";
                    int? addressId = null;
                    using (SqlCommand addrCmd = new SqlCommand(addressQuery, connection))
                    {
                        addrCmd.Parameters.AddWithValue("@orderId", orderId);
                        var result = await addrCmd.ExecuteScalarAsync();
                        if (result != null)
                        {
                            addressId = (int)result;
                        }
                    }

                    if (!addressId.HasValue)
                    {
                        return Json(new { success = false, message = "Aucune adresse de livraison trouvée pour cette commande." });
                    }

                    // Create delivery
                    string insertQuery = @"INSERT INTO Livraisons (idCommande, idAdresse, dateDebutEstimation, dateFinEstimation, created_at)
                                          VALUES (@orderId, @addressId, GETDATE(), DATEADD(day, 3, GETDATE()), GETDATE())";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@orderId", orderId);
                        cmd.Parameters.AddWithValue("@addressId", addressId.Value);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Update order status
                    string updateQuery = "UPDATE Commandes SET statut = 'en_livraison' WHERE idCommande = @orderId";
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@orderId", orderId);
                        await updateCmd.ExecuteNonQueryAsync();
                    }
                }

                return Json(new { success = true, message = "Livraison créée avec succès." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la livraison");
                return Json(new { success = false, message = "Erreur lors de la création de la livraison." });
            }
        }
    }
}