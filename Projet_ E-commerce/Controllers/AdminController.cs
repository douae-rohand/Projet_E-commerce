using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Projet__E_commerce.Data;
using Projet__E_commerce.Filters;
using Projet__E_commerce.Models;

namespace Projet__E_commerce
{
    [AuthorizeRole("ADMIN")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminController(ILogger<AdminController> logger, ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _logger = logger;
            _context = context;
            _environment = environment;
        }

        private int GetAdminId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        // ============ DASHBOARD ============
        public async Task<IActionResult> Dashboard()
        {
            var adminId = GetAdminId();
            var model = new AdminDashboardViewModel();

            try
            {
                // Get statistics
                var statsParam = new SqlParameter("@idAdmin", adminId);
                var stats = await _context.Database
                    .SqlQueryRaw<AdminStatisticsDto>(
                        "EXEC sp_get_admin_statistics @idAdmin",
                        statsParam)
                    .ToListAsync();

                if (stats.Any())
                {
                    var stat = stats.First();
                    model.VentesMois = stat.VentesMois;
                    model.VentesPourcentage = stat.VentesPourcentage;
                    model.CommandesMois = stat.CommandesMois;
                    model.CommandesPourcentage = stat.CommandesPourcentage;
                    model.ProduitsActifs = stat.ProduitsActifs;
                }

                // Get recent orders
                var ordersParam = new SqlParameter("@idAdmin", adminId);
                var limitParam = new SqlParameter("@limit", 10);
                model.RecentOrders = await _context.Database
                    .SqlQueryRaw<RecentOrderDto>(
                        "EXEC sp_get_admin_recent_orders @idAdmin, @limit",
                        ordersParam, limitParam)
                    .ToListAsync();

                // Get top products
                var topProductsParam = new SqlParameter("@idAdmin", adminId);
                var topLimitParam = new SqlParameter("@limit", 5);
                model.TopProducts = await _context.Database
                    .SqlQueryRaw<TopProductDto>(
                        "EXEC sp_get_admin_top_products @idAdmin, @limit",
                        topProductsParam, topLimitParam)
                    .ToListAsync();

                // Get admin profile for cooperative name and location
                var admin = await _context.Admins
                    .Where(a => a.id == adminId)
                    .FirstOrDefaultAsync();

                if (admin != null)
                {
                    model.CooperativeName = admin.nom_cooperative;
                    model.Location = $"{admin.ville}, Maroc";
                }

                ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
                ViewBag.UserId = adminId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["ErrorMessage"] = "Erreur lors du chargement du tableau de bord";
            }

            return View(model);
        }

        // ============ PRODUCTS ============
        public async Task<IActionResult> Products()
        {
            var adminId = GetAdminId();
            var products = new List<ProductListItemViewModel>();

            try
            {
                var param = new SqlParameter("@idAdmin", adminId);
                products = await _context.Database
                    .SqlQueryRaw<ProductListItemViewModel>(
                        "EXEC sp_get_products_by_admin @idAdmin",
                        param)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products");
                TempData["ErrorMessage"] = "Erreur lors du chargement des produits";
            }

            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserId = adminId;
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            var model = new ProductManagementViewModel
            {
                Categories = await _context.Categories.ToListAsync()
            };

            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserId = GetAdminId();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductManagementViewModel model, IFormFile? photoFile)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories.ToListAsync();
                return View(model);
            }

            try
            {
                var adminId = GetAdminId();
                string? photoPath = null;

                // Handle file upload
                if (photoFile != null && photoFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{photoFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await photoFile.CopyToAsync(fileStream);
                    }

                    photoPath = $"/uploads/products/{uniqueFileName}";
                }

                // Call stored procedure
                var parameters = new[]
                {
                    new SqlParameter("@idAdmin", adminId),
                    new SqlParameter("@nomP", model.NomP),
                    new SqlParameter("@description", (object?)model.Description ?? DBNull.Value),
                    new SqlParameter("@seuil_alerte", model.SeuilAlerte),
                    new SqlParameter("@idC", model.IdC),
                    new SqlParameter("@prix", model.Prix),
                    new SqlParameter("@taille", (object?)model.Taille ?? DBNull.Value),
                    new SqlParameter("@couleur", (object?)model.Couleur ?? DBNull.Value),
                    new SqlParameter("@photo", (object?)photoPath ?? DBNull.Value),
                    new SqlParameter("@quantite", model.Quantite),
                    new SqlParameter("@poids", (object?)model.Poids ?? DBNull.Value)
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_create_product @idAdmin, @nomP, @description, @seuil_alerte, @idC, @prix, @taille, @couleur, @photo, @quantite, @poids",
                    parameters);

                TempData["SuccessMessage"] = "Produit créé avec succès";
                return RedirectToAction(nameof(Products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                ModelState.AddModelError("", "Erreur lors de la création du produit");
                model.Categories = await _context.Categories.ToListAsync();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var adminId = GetAdminId();
            var product = await _context.Produits
                .Include(p => p.Categorie)
                .Include(p => p.Variantes)
                .Where(p => p.idP == id && p.idAdmin == adminId)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                TempData["ErrorMessage"] = "Produit non trouvé";
                return RedirectToAction(nameof(Products));
            }

            var model = new ProductManagementViewModel
            {
                IdP = product.idP,
                NomP = product.nomP,
                Description = product.description,
                SeuilAlerte = product.seuil_alerte,
                IdC = product.idC,
                Statut = product.statut,
                Categories = await _context.Categories.ToListAsync(),
                Variantes = product.Variantes.ToList()
            };

            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserId = adminId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductManagementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories.ToListAsync();
                return View(model);
            }

            try
            {
                var adminId = GetAdminId();
                var parameters = new[]
                {
                    new SqlParameter("@idP", model.IdP),
                    new SqlParameter("@idAdmin", adminId),
                    new SqlParameter("@nomP", model.NomP),
                    new SqlParameter("@description", (object?)model.Description ?? DBNull.Value),
                    new SqlParameter("@seuil_alerte", model.SeuilAlerte),
                    new SqlParameter("@idC", model.IdC),
                    new SqlParameter("@statut", model.Statut)
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_update_product @idP, @idAdmin, @nomP, @description, @seuil_alerte, @idC, @statut",
                    parameters);

                TempData["SuccessMessage"] = "Produit modifié avec succès";
                return RedirectToAction(nameof(Products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                ModelState.AddModelError("", "Erreur lors de la modification du produit");
                model.Categories = await _context.Categories.ToListAsync();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var adminId = GetAdminId();
                var parameters = new[]
                {
                    new SqlParameter("@idP", id),
                    new SqlParameter("@idAdmin", adminId)
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_delete_product @idP, @idAdmin",
                    parameters);

                TempData["SuccessMessage"] = "Produit supprimé avec succès";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                TempData["ErrorMessage"] = "Erreur lors de la suppression du produit";
            }

            return RedirectToAction(nameof(Products));
        }

        // ============ ORDERS ============
        public async Task<IActionResult> Orders(string? statut)
        {
            var adminId = GetAdminId();
            var orders = new List<OrderManagementViewModel>();

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@idAdmin", adminId),
                    new SqlParameter("@statut", (object?)statut ?? DBNull.Value)
                };

                // Utiliser un DTO simple pour éviter les problèmes de navigation EF Core
                var dtos = await _context.Database
                    .SqlQueryRaw<OrderListItemDto>(
                        "EXEC sp_get_orders_by_admin @idAdmin, @statut",
                        parameters)
                    .ToListAsync();

                // Convertir les DTOs en ViewModels
                orders = dtos.Select(d => new OrderManagementViewModel
                {
                    IdCommande = d.IdCommande,
                    NumeroCommande = d.NumeroCommande,
                    IdClient = d.IdClient,
                    NomClient = d.NomClient,
                    TelephoneClient = d.TelephoneClient,
                    Statut = d.Statut,
                    StatusLabel = d.StatusLabel,
                    StatusClass = d.StatusClass,
                    PrixTotal = d.PrixTotal,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt,
                    StatutLivraison = d.StatutLivraison,
                    ModeLivraison = d.ModeLivraison,
                    DateDebutEstimation = d.DateDebutEstimation,
                    DateFinEstimation = d.DateFinEstimation
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders");
                TempData["ErrorMessage"] = "Erreur lors du chargement des commandes";
            }

            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserId = adminId;
            ViewBag.CurrentFilter = statut;
            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var adminId = GetAdminId();
            OrderManagementViewModel? model = null;

            try
            {
                // 1. Get order info
                var parameters = new[]
                {
                    new SqlParameter("@idAdmin", adminId),
                    new SqlParameter("@statut", DBNull.Value),
                    new SqlParameter("@idCommande", id)
                };

                // Utiliser le DTO simple
                var dtos = await _context.Database
                    .SqlQueryRaw<OrderListItemDto>(
                        "EXEC sp_get_orders_by_admin @idAdmin, @statut, @idCommande",
                        parameters)
                    .ToListAsync();
                
                var orderDto = dtos.FirstOrDefault();

                if (orderDto == null)
                {
                    TempData["ErrorMessage"] = "Commande non trouvée";
                    return RedirectToAction(nameof(Orders));
                }

                model = new OrderManagementViewModel
                {
                    IdCommande = orderDto.IdCommande,
                    NumeroCommande = orderDto.NumeroCommande,
                    IdClient = orderDto.IdClient,
                    NomClient = orderDto.NomClient,
                    TelephoneClient = orderDto.TelephoneClient,
                    Statut = orderDto.Statut,
                    StatusLabel = orderDto.StatusLabel,
                    StatusClass = orderDto.StatusClass,
                    PrixTotal = orderDto.PrixTotal,
                    CreatedAt = orderDto.CreatedAt,
                    UpdatedAt = orderDto.UpdatedAt,
                    StatutLivraison = orderDto.StatutLivraison,
                    ModeLivraison = orderDto.ModeLivraison,
                    DateDebutEstimation = orderDto.DateDebutEstimation,
                    DateFinEstimation = orderDto.DateFinEstimation
                };

                // 2. Get items
                var itemsParam = new[]
                {
                    new SqlParameter("@idCommande", id),
                    new SqlParameter("@idAdmin", adminId)
                };

                var items = await _context.Database
                    .SqlQueryRaw<LigneCommandeDto>(
                        "EXEC sp_get_order_items_admin @idCommande, @idAdmin",
                        itemsParam)
                    .ToListAsync();

                model.LignesCommande = items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order details");
                TempData["ErrorMessage"] = "Erreur lors du chargement des détails de la commande";
                return RedirectToAction(nameof(Orders));
            }

            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserId = adminId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptOrder(int id)
        {
            try
            {
                var adminId = GetAdminId();
                var parameters = new[]
                {
                    new SqlParameter("@idCommande", id),
                    new SqlParameter("@idAdmin", adminId)
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_accept_order @idCommande, @idAdmin",
                    parameters);

                TempData["SuccessMessage"] = "Commande acceptée avec succès";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting order");
                TempData["ErrorMessage"] = "Erreur lors de l'acceptation de la commande";
            }

            return RedirectToAction(nameof(Orders));
        }

        // ============ PROFILE ============
        public async Task<IActionResult> Profile()
        {
            var adminId = GetAdminId();
            AdminProfileViewModel? model = null;

            try
            {
                var param = new SqlParameter("@idAdmin", adminId);
                var profiles = await _context.Database
                    .SqlQueryRaw<AdminProfileViewModel>(
                        "EXEC sp_get_admin_profile @idAdmin",
                        param)
                    .ToListAsync();

                model = profiles.FirstOrDefault();

                if (model == null)
                {
                    TempData["ErrorMessage"] = "Profil non trouvé";
                    return RedirectToAction(nameof(Dashboard));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile");
                TempData["ErrorMessage"] = "Erreur lors du chargement du profil";
                return RedirectToAction(nameof(Dashboard));
            }

            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserId = adminId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(AdminProfileViewModel model, IFormFile? logoFile)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var adminId = GetAdminId();
                string? logoPath = model.Logo;

                // Handle logo upload
                if (logoFile != null && logoFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "logos");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{logoFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await logoFile.CopyToAsync(fileStream);
                    }

                    logoPath = $"/uploads/logos/{uniqueFileName}";
                }

                var parameters = new[]
                {
                    new SqlParameter("@idAdmin", adminId),
                    new SqlParameter("@nom_cooperative", model.NomCooperative),
                    new SqlParameter("@localisation", (object?)model.Localisation ?? DBNull.Value),
                    new SqlParameter("@ville", (object?)model.Ville ?? DBNull.Value),
                    new SqlParameter("@telephone", (object?)model.Telephone ?? DBNull.Value),
                    new SqlParameter("@description", (object?)model.Description ?? DBNull.Value),
                    new SqlParameter("@logo", (object?)logoPath ?? DBNull.Value)
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_update_admin_profile @idAdmin, @nom_cooperative, @localisation, @ville, @telephone, @description, @logo",
                    parameters);

                TempData["SuccessMessage"] = "Profil mis à jour avec succès";
                return RedirectToAction(nameof(Profile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                ModelState.AddModelError("", "Erreur lors de la mise à jour du profil");
                return View(model);
            }
        }
    }
}
