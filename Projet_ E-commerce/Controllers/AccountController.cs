using Microsoft.AspNetCore.Mvc;

namespace Projet__E_commerce.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult UserDashboard()
        {
            // Vérifier si l'utilisateur est Client
            if (HttpContext.Session.GetString("UserRole") != "CLIENT")
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }
    }
}
