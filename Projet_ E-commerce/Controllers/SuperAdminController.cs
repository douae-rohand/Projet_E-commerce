// SuperAdminController.cs
using Microsoft.AspNetCore.Mvc;
using Projet__E_commerce.Filters;

namespace Projet__E_commerce.Controllers
{
    [AuthorizeRole("SUPER_ADMIN")]
    public class SuperAdminController : Controller
    {
        private readonly ILogger<SuperAdminController> _logger;

        public SuperAdminController(ILogger<SuperAdminController> logger)
        {
            _logger = logger;
        }

        public IActionResult Dashboard()
        {
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            return View();
        }

        public IActionResult ManageUsers()
        {
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            return View();
        }

        public IActionResult ManageCooperatives()
        {
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            return View();
        }

        public IActionResult Statistics()
        {
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            return View();
        }
    }
}