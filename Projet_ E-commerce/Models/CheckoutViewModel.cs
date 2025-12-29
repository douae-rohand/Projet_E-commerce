using System.ComponentModel.DataAnnotations;

namespace Projet__E_commerce.Models
{
    public class CheckoutViewModel
    {
        // Existing Address Selection (or New)
        public int? SelectedAddressId { get; set; }
        public List<AdresseLivraison> ExistingAddresses { get; set; } = new();

        // New Address Fields
        public string? NewAddressName { get; set; } // "Maison"
       
        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string ZipCode { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        // Delivery
        public string DeliveryMode { get; set; } = "Standard"; // Standard or Express
        public string PaymentMethod { get; set; } = "COD";
    }
}
