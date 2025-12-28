using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;

namespace Projet__E_commerce.Controllers
{
    public class authController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public authController(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View("~/Views/Account/Login.cshtml");
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe = false)
        {
            // Nettoyage des entrées
            email = email?.Trim();
            password = password?.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["ErrorMessage"] = "Email et mot de passe requis.";
                return View("~/Views/Account/Login.cshtml");
            }

            try
            {
                // Hachage du mot de passe pour la vérification (même méthode que l'inscription)
                // Utilisation de majuscules pour correspondre au style SQL Server (CONVERT(..., 2))
                string hashedPassword = HashPassword(password, Encoding.UTF8);
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand("sp_login_utilisateur", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@email", email);
                        command.Parameters.AddWithValue("@password", hashedPassword);

                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int userId = reader.GetInt32(reader.GetOrdinal("id"));
                                string userEmail = reader.GetString(reader.GetOrdinal("email"));
                                string role = reader.GetString(reader.GetOrdinal("role"));

                                // Connexion (Session)
                                HttpContext.Session.SetInt32("UserId", userId);
                                HttpContext.Session.SetString("UserEmail", userEmail);
                                HttpContext.Session.SetString("UserRole", role);

                                // Redirection selon le rôle
                                switch (role)
                                {
                                    case "SUPER_ADMIN":
                                        return RedirectToAction("Dashboard", "SuperAdmin");
                                    case "ADMIN":
                                        return RedirectToAction("Dashboard", "Admin");
                                    case "CLIENT":
                                        return RedirectToAction("UserDashboard", "Account");
                                    default:
                                        return RedirectToAction("UserDashboard", "Account");
                                }
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Email ou mot de passe incorrect.";
                                return View("~/Views/Account/Login.cshtml");
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("~/Views/Account/Login.cshtml");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Une erreur s'est produite lors de la connexion.";
                return View("~/Views/Account/Login.cshtml");
            }
        }
        

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View("~/Views/Account/Register.cshtml");
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string userType,
            string email,
            string password,
            string confirmPassword,
            // Champs Client
            string? prenom,
            string? nom,
            DateTime? date_naissance,
            // Champs Admin
            string? nom_cooperative,
            string? ville,
            string? localisation,
            string? description,
            IFormFile? logo,
            // Commun
            string? telephone)
        {
            email = email?.Trim();
            password = password?.Trim();
            confirmPassword = confirmPassword?.Trim();

            // Sauvegarder les données pour réaffichage en cas d'erreur
            ViewData["UserType"] = userType;
            ViewData["Email"] = email;
            ViewData["Prenom"] = prenom;
            ViewData["Nom"] = nom;
            ViewData["DateNaissance"] = date_naissance;
            ViewData["NomCooperative"] = nom_cooperative;
            ViewData["Ville"] = ville;
            ViewData["Localisation"] = localisation;
            ViewData["Description"] = description;
            ViewData["Telephone"] = telephone;
            ViewData["Password"] = password;
            ViewData["ConfirmPassword"] = confirmPassword;

            try
            {
                if (password != confirmPassword)
                {
                    TempData["ErrorMessage"] = "Les mots de passe ne correspondent pas.";
                    return View("~/Views/Account/Register.cshtml");
                }

                // Hachage standard pour l'inscription (UTF8, majuscule pour correspondre à SQL Server)
                string hashedPassword = HashPassword(password, Encoding.UTF8);
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                if (userType == "client")
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        using (SqlCommand command = new SqlCommand("sp_inscription_client", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = email;
                            command.Parameters.Add("@password", SqlDbType.NVarChar, 255).Value = hashedPassword;
                            command.Parameters.Add("@prenom", SqlDbType.NVarChar, 255).Value = prenom ?? string.Empty;
                            command.Parameters.Add("@nom", SqlDbType.NVarChar, 255).Value = nom ?? string.Empty;
                            command.Parameters.Add("@telephone", SqlDbType.NVarChar, 100).Value = telephone ?? (object)DBNull.Value;
                            command.Parameters.Add("@date_naissance", SqlDbType.DateTime2).Value = date_naissance ?? (object)DBNull.Value;

                            await connection.OpenAsync();

                            using (SqlDataReader reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    string message = reader.GetString(reader.GetOrdinal("message"));
                                    int newUserId = reader.GetInt32(reader.GetOrdinal("id"));

                                    if (message == "INSCRIPTION_CLIENT_OK")
                                    {
                                        HttpContext.Session.SetInt32("UserId", newUserId);
                                        HttpContext.Session.SetString("UserEmail", email);
                                        HttpContext.Session.SetString("UserRole", "CLIENT");

                                        TempData["SuccessMessage"] = "Inscription réussie ! Bienvenue.";
                                        return RedirectToAction("UserDashboard", "Account");
                                    }
                                }
                            }
                        }
                    }
                }
                else if (userType == "admin")
                {
                    string? logoPath = null;
                    if (logo != null && logo.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "logos");
                        Directory.CreateDirectory(uploadsFolder);
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + logo.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await logo.CopyToAsync(fileStream);
                        }
                        logoPath = "/uploads/logos/" + uniqueFileName;
                    }

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        using (SqlCommand command = new SqlCommand("sp_inscription_admin", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@email", email);
                            command.Parameters.AddWithValue("@password", hashedPassword);
                            command.Parameters.AddWithValue("@nom_cooperative", nom_cooperative ?? string.Empty);
                            command.Parameters.AddWithValue("@localisation", localisation ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@ville", ville ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@logo", logoPath ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@telephone", telephone ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@description", description ?? (object)DBNull.Value);

                            await connection.OpenAsync();

                            using (SqlDataReader reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    string message = reader.GetString(reader.GetOrdinal("message"));
                                    int newUserId = reader.GetInt32(reader.GetOrdinal("id"));

                                    if (message == "INSCRIPTION_ADMIN_OK")
                                    {
                                        HttpContext.Session.SetInt32("UserId", newUserId);
                                        HttpContext.Session.SetString("UserEmail", email);
                                        HttpContext.Session.SetString("UserRole", "ADMIN");

                                        TempData["SuccessMessage"] = "Inscription réussie ! Bienvenue.";
                                        return RedirectToAction("Dashboard", "Admin");
                                    }
                                }
                            }
                        }
                    }
                }

                TempData["ErrorMessage"] = "Une erreur s'est produite lors de l'inscription.";
                return View("~/Views/Account/Register.cshtml");
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = "Erreur base de données : " + ex.Message;
                return View("~/Views/Account/Register.cshtml");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erreur : " + ex.Message;
                return View("~/Views/Account/Register.cshtml");
            }
        }

        // GET: /Auth/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/");
        }

        // Méthode utilitaire de hachage
        private string HashPassword(string password, Encoding encoding)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(encoding.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("X2"));
                }
                return builder.ToString();
            }
        }
    }
}