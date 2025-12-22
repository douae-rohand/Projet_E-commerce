using Microsoft.AspNetCore.Mvc;

namespace Projet__E_commerce.Controllers
{
    public class AuthController : Controller
    {
        // Page de connexion (par défaut)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Traitement de la connexion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password, bool rememberMe = false)
        {
            if (ModelState.IsValid)
            {
                // TODO: Ajouter votre logique d'authentification ici
                // Exemple : vérifier email/password dans la base de données

                // Si la connexion réussit :
                // return RedirectToAction("Index", "Home");

                // Pour le moment, redirection temporaire
                TempData["SuccessMessage"] = "Connexion réussie !";
                return RedirectToAction("Index", "Home");
            }

            // Si échec, retourner à la page de connexion avec erreur
            ModelState.AddModelError("", "Email ou mot de passe incorrect");
            return View();
        }

        // Page d'inscription
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Traitement de l'inscription
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string firstName, string lastName, string email,
                                     string phone, string password, string confirmPassword,
                                     bool acceptTerms = false)
        {
            if (ModelState.IsValid)
            {
                // Vérifier que les mots de passe correspondent
                if (password != confirmPassword)
                {
                    ModelState.AddModelError("", "Les mots de passe ne correspondent pas");
                    return View();
                }

                // Vérifier que les termes sont acceptés
                if (!acceptTerms)
                {
                    ModelState.AddModelError("", "Vous devez accepter les conditions générales");
                    return View();
                }

                // TODO: Ajouter votre logique d'inscription ici
                // Exemple : créer un nouvel utilisateur dans la base de données

                // Si l'inscription réussit, rediriger vers la page de connexion
                TempData["SuccessMessage"] = "Inscription réussie ! Connectez-vous maintenant.";
                return RedirectToAction("Login");
            }

            return View();
        }

        // Page mot de passe oublié
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // Traitement mot de passe oublié
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string email)
        {
            if (ModelState.IsValid)
            {
                // TODO: Envoyer un email de réinitialisation

                TempData["SuccessMessage"] = "Un email de réinitialisation a été envoyé.";
                return RedirectToAction("Login");
            }

            return View();
        }

        // Déconnexion
        [HttpPost]
        public IActionResult Logout()
        {
            // TODO: Ajouter votre logique de déconnexion ici
            // Exemple : effacer la session, les cookies, etc.

            TempData["SuccessMessage"] = "Vous êtes déconnecté.";
            return RedirectToAction("Login");
        }

        // Redirection depuis Index vers Login
        public IActionResult Index()
        {
            return RedirectToAction("Login");
        }
    }
}