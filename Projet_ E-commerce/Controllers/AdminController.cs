using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Projet__E_commerce.Data;
using Projet__E_commerce.Filters;
using Projet__E_commerce.Models;
using System.IO;

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

        private async Task<string?> SaveUploadedFileAsync(IFormFile? file, string subfolder)
        {
            if (file == null || file.Length == 0) return null;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", subfolder);
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{subfolder.Replace("\\", "/")}/{uniqueFileName}";
        }

        // ============ DASHBOARD ============
        public async Task<IActionResult> Dashboard()
        {
            var adminId = GetAdminId();
            var model = new AdminDashboardViewModel();

            try
            {
                var now = DateTime.Now;
                var currentMonthStart = new DateTime(now.Year, now.Month, 1);
                var lastMonthStart = currentMonthStart.AddMonths(-1);
                var lastMonthEnd = currentMonthStart.AddDays(-1);

                // Calculate sales for current month - TEMPORARILY INCLUDE ALL STATUSES FOR DEBUGGING
                var currentMonthOrders = await _context.Commandes
                    .Include(c => c.LignesCommande)
                        .ThenInclude(lc => lc.Variante)
                            .ThenInclude(v => v.Produit)
                    .Where(c => c.created_at >= currentMonthStart &&
                               c.LignesCommande.Any(lc => lc.Variante.Produit.idAdmin == adminId))
                    .ToListAsync();

                _logger.LogInformation($"Dashboard Debug - Current month orders found: {currentMonthOrders.Count}");
                _logger.LogInformation($"Dashboard Debug - Admin ID: {adminId}");

                var currentMonthSales = currentMonthOrders
                    .SelectMany(c => c.LignesCommande.Where(lc => lc.Variante.Produit.idAdmin == adminId))
                    .Sum(lc => lc.quantite * lc.prix_unitaire);

                _logger.LogInformation($"Dashboard Debug - Current month sales: {currentMonthSales}");

                // Calculate sales for last month
                var lastMonthOrders = await _context.Commandes
                    .Include(c => c.LignesCommande)
                        .ThenInclude(lc => lc.Variante)
                            .ThenInclude(v => v.Produit)
                    .Where(c => c.created_at >= lastMonthStart && c.created_at <= lastMonthEnd &&
                               c.LignesCommande.Any(lc => lc.Variante.Produit.idAdmin == adminId))
                    .ToListAsync();

                var lastMonthSales = lastMonthOrders
                    .SelectMany(c => c.LignesCommande.Where(lc => lc.Variante.Produit.idAdmin == adminId))
                    .Sum(lc => lc.quantite * lc.prix_unitaire);

                model.VentesMois = currentMonthSales;
                model.VentesPourcentage = lastMonthSales > 0 
                    ? ((currentMonthSales - lastMonthSales) / lastMonthSales) * 100 
                    : 0;

                // Count orders for current month
                var currentMonthOrdersCount = await _context.Commandes
                    .Where(c => c.created_at >= currentMonthStart &&
                               c.LignesCommande.Any(lc => lc.Variante.Produit.idAdmin == adminId))
                    .CountAsync();

                // Count orders for last month
                var lastMonthOrdersCount = await _context.Commandes
                    .Where(c => c.created_at >= lastMonthStart && c.created_at <= lastMonthEnd &&
                               c.LignesCommande.Any(lc => lc.Variante.Produit.idAdmin == adminId))
                    .CountAsync();

                model.CommandesMois = currentMonthOrdersCount;
                model.CommandesPourcentage = lastMonthOrdersCount > 0
                    ? ((decimal)(currentMonthOrdersCount - lastMonthOrdersCount) / lastMonthOrdersCount) * 100
                    : 0;

                // Count active products
                model.ProduitsActifs = await _context.Produits
                    .Where(p => p.idAdmin == adminId && p.statut == "active")
                    .CountAsync();

                // Get recent orders (last 10)
                var recentOrders = await _context.Commandes
                    .Include(c => c.Client)
                    .Include(c => c.LignesCommande)
                        .ThenInclude(lc => lc.Variante)
                            .ThenInclude(v => v.Produit)
                    .Where(c => c.LignesCommande.Any(lc => lc.Variante.Produit.idAdmin == adminId))
                    .OrderByDescending(c => c.created_at)
                    .Take(10)
                    .ToListAsync();

                model.RecentOrders = recentOrders.Select(c => new RecentOrderDto
                {
                    IdCommande = c.idCommande,
                    NumeroCommande = $"CMD-{c.idCommande:D6}",
                    NomClient = c.Client.nom,
                    Statut = c.statut,
                    PrixTotal = c.LignesCommande
                        .Where(lc => lc.Variante.Produit.idAdmin == adminId)
                        .Sum(lc => lc.quantite * lc.prix_unitaire),
                    CreatedAt = c.created_at
                }).ToList();

                // Get top 5 products by sales - INCLUDE ALL STATUSES FOR DEBUGGING
                var topProducts = await _context.LignesCommande
                    .Include(lc => lc.Variante)
                        .ThenInclude(v => v.Produit)
                    .Include(lc => lc.Commande)
                    .Where(lc => lc.Variante.Produit.idAdmin == adminId)
                    .GroupBy(lc => new { lc.Variante.Produit.idP, lc.Variante.Produit.nomP })
                    .Select(g => new TopProductDto
                    {
                        IdP = g.Key.idP,
                        NomP = g.Key.nomP,
                        NombreVentes = g.Sum(lc => lc.quantite)
                    })
                    .OrderByDescending(p => p.NombreVentes)
                    .Take(5)
                    .ToListAsync();

                _logger.LogInformation($"Dashboard Debug - Top products count: {topProducts.Count}");

                model.TopProducts = topProducts;

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
                // Get products with variants using EF
                var produitsQuery = await _context.Produits
                    .Include(p => p.Categorie)
                    .Include(p => p.Variantes)
                    .Where(p => p.idAdmin == adminId)
                    .ToListAsync();

                products = produitsQuery.Select(p => new ProductListItemViewModel
                {
                    IdP = p.idP,
                    NomP = p.nomP,
                    Description = p.description,
                    NomCategorie = p.Categorie?.nom ?? "Sans catégorie",
                    Statut = p.statut,
                    SeuilAlerte = p.seuil_alerte,
                    NombreVariantes = p.Variantes.Count,
                    StockTotal = p.Variantes.Sum(v => v.quantite),
                    PrixMin = p.Variantes.Any() ? p.Variantes.Min(v => v.prix) : 0,
                    PrixMax = p.Variantes.Any() ? p.Variantes.Max(v => v.prix) : 0,
                    // Check if ANY variant has low stock
                    HasLowStock = p.Variantes.Any(v => v.quantite > 0 && v.quantite <= p.seuil_alerte),
                    // Get all variant photos for carousel
                    VariantPhotos = p.Variantes
                        .Where(v => !string.IsNullOrEmpty(v.photo))
                        .Select(v => v.photo)
                        .Distinct()
                        .ToList()
                })
                .OrderBy(p => p.Statut == "inactive" ? 1 : 0) // Active products first
                .ThenByDescending(p => p.IdP) // Then by ID descending
                .ToList();
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
        public async Task<IActionResult> ProductDetails(int id)
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
                NomCategorie = product.Categorie?.nom,
                Statut = product.statut,
                Variantes = product.Variantes.ToList(),
                StockTotal = product.Variantes.Sum(v => v.quantite),
                PrixMin = product.Variantes.Any() ? product.Variantes.Min(v => v.prix) : null,
                PrixMax = product.Variantes.Any() ? product.Variantes.Max(v => v.prix) : null
            };

            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserId = adminId;
            return View(model);
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
                
                // Find product and verify ownership
                var product = await _context.Produits
                    .Where(p => p.idP == id && p.idAdmin == adminId)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    TempData["ErrorMessage"] = "Produit non trouvé";
                    return RedirectToAction(nameof(Products));
                }

                // Soft delete: change status to inactive
                product.statut = "inactive";
                product.updated_at = DateTime.Now;
                
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Produit désactivé avec succès";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                TempData["ErrorMessage"] = "Erreur lors de la suppression du produit";
            }

            return RedirectToAction(nameof(Products));
        }

        // ============ VARIANT MANAGEMENT ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVariant(int productId, decimal prix, int quantite, string? taille, string? couleur, string? poids, IFormFile? photoFile)
        {
            try
            {
                var adminId = GetAdminId();

                // Verify product belongs to admin
                var product = await _context.Produits
                    .Where(p => p.idP == productId && p.idAdmin == adminId)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    TempData["ErrorMessage"] = "Produit non trouvé";
                    return RedirectToAction(nameof(Products));
                }

                // Handle photo upload
                string? photoPath = null;
                if (photoFile != null && photoFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + photoFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await photoFile.CopyToAsync(fileStream);
                    }
                    photoPath = "/uploads/products/" + uniqueFileName;
                }

                // Create new variant
                var variante = new Variante
                {
                    idP = productId,
                    prix = prix,
                    quantite = quantite,
                    taille = taille,
                    couleur = couleur,
                    poids = poids,
                    photo = photoPath,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };

                _context.Variantes.Add(variante);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Variante ajoutée avec succès";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding variant");
                TempData["ErrorMessage"] = "Erreur lors de l'ajout de la variante";
            }

            return RedirectToAction(nameof(EditProduct), new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVariant(int idV, int productId, decimal prix, int quantite, string? taille, string? couleur, string? poids, IFormFile? photoFile)
        {
            try
            {
                var adminId = GetAdminId();

                // Verify variant belongs to admin's product
                var variante = await _context.Variantes
                    .Include(v => v.Produit)
                    .Where(v => v.idV == idV && v.Produit.idAdmin == adminId)
                    .FirstOrDefaultAsync();

                if (variante == null)
                {
                    TempData["ErrorMessage"] = "Variante non trouvée";
                    return RedirectToAction(nameof(Products));
                }

                // Update fields
                variante.prix = prix;
                variante.quantite = quantite;
                variante.taille = taille;
                variante.couleur = couleur;
                variante.poids = poids;
                variante.updated_at = DateTime.Now;

                // Handle photo upload if provided
                if (photoFile != null && photoFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + photoFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await photoFile.CopyToAsync(fileStream);
                    }
                    variante.photo = "/uploads/products/" + uniqueFileName;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Variante modifiée avec succès";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating variant");
                TempData["ErrorMessage"] = "Erreur lors de la modification de la variante";
            }

            return RedirectToAction(nameof(EditProduct), new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVariant(int idV, int productId)
        {
            try
            {
                var adminId = GetAdminId();

                // Verify variant belongs to admin's product
                var variante = await _context.Variantes
                    .Include(v => v.Produit)
                    .Where(v => v.idV == idV && v.Produit.idAdmin == adminId)
                    .FirstOrDefaultAsync();

                if (variante == null)
                {
                    TempData["ErrorMessage"] = "Variante non trouvée";
                    return RedirectToAction(nameof(Products));
                }

                _context.Variantes.Remove(variante);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Variante supprimée avec succès";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting variant");
                TempData["ErrorMessage"] = "Erreur lors de la suppression de la variante";
            }

            return RedirectToAction(nameof(EditProduct), new { id = productId });
        }

        // ============ ORDERS ============
        public async Task<IActionResult> Orders(string? statut)
        {
            var adminId = GetAdminId();
            var orders = new List<OrderManagementViewModel>();

            try
            {
                // Auto-accepter les commandes en attente si le stock est suffisant
                await AutoAcceptPendingOrdersAsync(adminId);

                // Récupérer les commandes avec les produits de cette coopérative
                var commandesQuery = _context.Commandes
                    .Include(c => c.Client)
                    .Include(c => c.LignesCommande)
                        .ThenInclude(lc => lc.Variante)
                            .ThenInclude(v => v.Produit)
                    .Include(c => c.Livraison)
                    .Where(c => c.LignesCommande.Any(lc => lc.Variante.Produit.idAdmin == adminId));

                // Filtrer par statut si fourni
                if (!string.IsNullOrEmpty(statut))
                {
                    commandesQuery = commandesQuery.Where(c => c.statut == statut);
                }

                var commandes = await commandesQuery
                    .OrderByDescending(c => c.created_at)
                    .ToListAsync();

                // Convertir en ViewModels
                orders = commandes.Select(c =>
                {
                    // Calculer le total pour cette coopérative uniquement
                    var lignesAdmin = c.LignesCommande
                        .Where(lc => lc.Variante.Produit.idAdmin == adminId)
                        .ToList();

                    var prixTotalAdmin = lignesAdmin.Sum(lc => lc.quantite * lc.prix_unitaire);

                    // Obtenir la première photo des produits de cette coopérative
                    var thumbnail = lignesAdmin
                        .Select(lc => lc.Variante.photo)
                        .FirstOrDefault(p => !string.IsNullOrEmpty(p));

                    return new OrderManagementViewModel
                    {
                        IdCommande = c.idCommande,
                        NumeroCommande = $"CMD-{c.idCommande:D6}",
                        IdClient = c.idClient,
                        NomClient = c.Client.nom,
                        TelephoneClient = c.Client.telephone,
                        Statut = c.statut,
                        StatusLabel = GetStatusLabel(c.statut),
                        StatusClass = GetStatusClass(c.statut),
                        PrixTotal = c.prixTotal,
                        PrixTotalAdmin = prixTotalAdmin,
                        Thumbnail = thumbnail,
                        CreatedAt = c.created_at,
                        UpdatedAt = c.updated_at,
                        StatutLivraison = c.Livraison?.statut,
                        ModeLivraison = c.Livraison?.mode_livraison,
                        DateDebutEstimation = c.Livraison?.dateDebutEstimation,
                        DateFinEstimation = c.Livraison?.dateFinEstimation
                    };
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

        private async Task AutoAcceptPendingOrdersAsync(int adminId)
        {
            // Récupérer les commandes en attente avec les produits de cette coopérative
            var pendingOrders = await _context.Commandes
                .Include(c => c.LignesCommande)
                    .ThenInclude(lc => lc.Variante)
                        .ThenInclude(v => v.Produit)
                .Where(c => c.statut == "en_attente" && 
                           c.LignesCommande.Any(lc => lc.Variante.Produit.idAdmin == adminId))
                .ToListAsync();

            foreach (var commande in pendingOrders)
            {
                // Vérifier uniquement les lignes de cette coopérative
                var lignesAdmin = commande.LignesCommande
                    .Where(lc => lc.Variante.Produit.idAdmin == adminId)
                    .ToList();

                bool stockSuffisant = true;

                // Vérifier le stock pour chaque ligne
                foreach (var ligne in lignesAdmin)
                {
                    if (ligne.Variante.quantite < ligne.quantite)
                    {
                        stockSuffisant = false;
                        break;
                    }
                }

                // Si le stock est suffisant, accepter la commande et déduire le stock
                if (stockSuffisant)
                {
                    foreach (var ligne in lignesAdmin)
                    {
                        ligne.Variante.quantite -= ligne.quantite;
                        ligne.Variante.updated_at = DateTime.Now;
                    }

                    commande.statut = "acceptée";
                    commande.updated_at = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
        }

        private string GetStatusLabel(string statut)
        {
            return statut switch
            {
                "en_attente" => "En attente",
                "acceptée" => "Acceptée",
                "en_cours" => "En cours",
                "livrée" => "Livrée",
                "annulée" => "Annulée",
                _ => statut
            };
        }

        private string GetStatusClass(string statut)
        {
            return statut switch
            {
                "en_attente" => "warning",
                "acceptée" => "info",
                "en_cours" => "primary",
                "livrée" => "success",
                "annulée" => "danger",
                _ => "secondary"
            };
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var adminId = GetAdminId();
            OrderManagementViewModel? model = null;

            try
            {
                // Récupérer la commande avec toutes les relations nécessaires
                var commande = await _context.Commandes
                    .Include(c => c.Client)
                    .Include(c => c.LignesCommande)
                        .ThenInclude(lc => lc.Variante)
                            .ThenInclude(v => v.Produit)
                    .Include(c => c.Livraison)
                    .Where(c => c.idCommande == id && 
                               c.LignesCommande.Any(lc => lc.Variante.Produit.idAdmin == adminId))
                    .FirstOrDefaultAsync();

                if (commande == null)
                {
                    TempData["ErrorMessage"] = "Commande non trouvée";
                    return RedirectToAction(nameof(Orders));
                }

                // Filtrer les lignes pour n'inclure que les produits de cette coopérative
                var lignesAdmin = commande.LignesCommande
                    .Where(lc => lc.Variante.Produit.idAdmin == adminId)
                    .ToList();

                // Calculer le total pour cette coopérative
                var prixTotalAdmin = lignesAdmin.Sum(lc => lc.quantite * lc.prix_unitaire);

                model = new OrderManagementViewModel
                {
                    IdCommande = commande.idCommande,
                    NumeroCommande = $"CMD-{commande.idCommande:D6}",
                    IdClient = commande.idClient,
                    NomClient = commande.Client.nom,
                    TelephoneClient = commande.Client.telephone,
                    Statut = commande.statut,
                    StatusLabel = GetStatusLabel(commande.statut),
                    StatusClass = GetStatusClass(commande.statut),
                    PrixTotal = commande.prixTotal,
                    PrixTotalAdmin = prixTotalAdmin,
                    CreatedAt = commande.created_at,
                    UpdatedAt = commande.updated_at,
                    StatutLivraison = commande.Livraison?.statut,
                    ModeLivraison = commande.Livraison?.mode_livraison,
                    DateDebutEstimation = commande.Livraison?.dateDebutEstimation,
                    DateFinEstimation = commande.Livraison?.dateFinEstimation,
                    
                    // Convertir les lignes de commande en DTOs
                    LignesCommande = lignesAdmin.Select(lc => new LigneCommandeDto
                    {
                        IdLC = lc.idLC,
                        NomProduit = lc.Variante.Produit.nomP,
                        Taille = lc.Variante.taille,
                        Couleur = lc.Variante.couleur,
                        Quantite = lc.quantite,
                        PrixUnitaire = lc.prix_unitaire,
                        Photo = lc.Variante.photo
                    }).ToList()
                };
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
