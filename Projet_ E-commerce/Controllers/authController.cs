using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Projet__E_commerce.Data;
using System.Data;
using System.Globalization;

namespace Projet__E_commerce.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AuthController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        private class LoginResult
        {
            public int id { get; set; }
            public string email { get; set; } = string.Empty;
            public string role { get; set; } = string.Empty;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe = false)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email et mot de passe sont requis.");
                return View();
            }

            try
            {
                var emailParam = new SqlParameter("@email", email);
                var passwordParam = new SqlParameter("@password", password);

                var loginResults = await _context.Database
                    .SqlQueryRaw<LoginResult>("EXEC sp_login_utilisateur @email, @password", emailParam, passwordParam)
                    .ToListAsync();

                var userResult = loginResults.FirstOrDefault();

                if (userResult != null)
                {
                    HttpContext.Session.SetInt32("UserId", userResult.id);
                    HttpContext.Session.SetString("UserRole", userResult.role);
                    HttpContext.Session.SetString("UserEmail", userResult.email);

                    TempData["SuccessMessage"] = "Connexion réussie !";

                    return userResult.role switch
                    {
                        "SUPER_ADMIN" => RedirectToAction("SuperAdminDashboard", "Admin"),
                        "ADMIN" => RedirectToAction("Dashboard", "Admin"),
                        _ => RedirectToAction("UserDashboard", "Account"),
                    };
                }
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }

            ModelState.AddModelError("", "Email ou mot de passe incorrect");
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string userType,
            string email,
            string password,
            string confirmPassword,
            string? prenom,
            string? nom,
            string? telephone,
            DateTime? date_naissance,
            string? nom_cooperative,
            string? localisation,
            string? ville,
            IFormFile? logo,
            bool acceptTerms = false)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Les mots de passe ne correspondent pas.");
                return View();
            }

            if (!acceptTerms)
            {
                ModelState.AddModelError("", "Vous devez accepter les conditions générales.");
                return View();
            }

            // Parsing robuste de la date de naissance
            if (!date_naissance.HasValue)
            {
                var birthDateStr = Request.Form["date_naissance"];
                if (!string.IsNullOrWhiteSpace(birthDateStr))
                {
                    if (DateTime.TryParse(birthDateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                    {
                        date_naissance = parsed;
                    }
                }
            }

            try
            {
                string? logoPath = null;
                if (logo != null && logo.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "logos");
                    if (!Directory.Exists(uploadsFolder)) 
                        Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + logo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await logo.CopyToAsync(fileStream);
                    }
                    
                    logoPath = "/uploads/logos/" + uniqueFileName;
                }

                if (userType == "admin")
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@email", email),
                        new SqlParameter("@password", password),
                        new SqlParameter("@nom_cooperative", nom_cooperative ?? (object)DBNull.Value),
                        new SqlParameter("@localisation", localisation ?? (object)DBNull.Value),
                        new SqlParameter("@ville", ville ?? (object)DBNull.Value),
                        new SqlParameter("@logo", logoPath ?? (object)DBNull.Value),
                        new SqlParameter("@telephone", telephone ?? (object)DBNull.Value)
                    };

                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC sp_inscription_admin @email, @password, @nom_cooperative, @localisation, @ville, @logo, @telephone",
                        parameters);
                }
                else
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@email", email),
                        new SqlParameter("@password", password),
                        new SqlParameter("@prenom", prenom ?? (object)DBNull.Value),
                        new SqlParameter("@nom", nom ?? (object)DBNull.Value),
                        new SqlParameter("@telephone", telephone ?? (object)DBNull.Value),
                        new SqlParameter("@date_naissance", date_naissance ?? (object)DBNull.Value)
                    };

                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC sp_inscription_client @email, @password, @prenom, @nom, @telephone, @date_naissance",
                        parameters);
                }

                // Auto-login après inscription réussie
                return await Login(email, password);
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Vous êtes déconnecté.";
            return RedirectToAction("Login");
        }
    }
}