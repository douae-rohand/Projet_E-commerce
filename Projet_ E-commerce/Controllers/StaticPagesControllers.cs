using Microsoft.AspNetCore.Mvc;

namespace Projet__E_commerce.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Details(int? id)
        {
            return View("~/Views/Product/Detail.cshtml");
        }
    }

    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }

    public class PartnersController : Controller
    {
        public IActionResult Join()
        {
            return Content("Coming soon");
        }
    }

    public class CooperativesController : Controller
    {
        public IActionResult Index()
        {
            return Content("Coming soon");
        }
    }

    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return Content("Coming soon");
        }
    }

    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            return Content("Coming soon");
        }
    }

    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return Content("Coming soon");
        }
    }

    public class HelpController : Controller
    {
        public IActionResult Faq()
        {
            return Content("Coming soon");
        }

        public IActionResult Shipping()
        {
            return Content("Coming soon");
        }

        public IActionResult Returns()
        {
            return Content("Coming soon");
        }
    }

    public class NewsletterController : Controller
    {
        [HttpPost]
        public IActionResult Subscribe(string? email)
        {
            return Content("Thanks for subscribing");
        }
    }
}
