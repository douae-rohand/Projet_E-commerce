using Microsoft.AspNetCore.Mvc;

namespace Projet__E_commerce.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            // Vérifier si l'utilisateur est Admin
            if (HttpContext.Session.GetString("UserRole") != "ADMIN")
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }

        public IActionResult SuperAdminDashboard()
        {
            // Vérifier si l'utilisateur est SuperAdmin
            if (HttpContext.Session.GetString("UserRole") != "SUPER_ADMIN")
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }
    }
}
