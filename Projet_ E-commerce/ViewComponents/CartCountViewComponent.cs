using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projet__E_commerce.Data;

namespace Projet__E_commerce.ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public CartCountViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            
            if (userId == null)
            {
                return View(0);
            }

            // Count items in cart (LignesCommande with status "en_attente")
            var count = await _db.LignesCommande
                .Where(lc => lc.Commande.idClient == userId && lc.Commande.statut == "en_attente")
                .SumAsync(lc => lc.quantite);

            return View(count);
        }
    }
}
