using ECommerce.API.Models;

namespace ECommerce.API.DTO
{
    public class ProductDto
    {

        public int? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ProductCategory { get; set; }
        public int Offer { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public string? Image { get; set; }
        public IFormFile file { get; set; } = null!;

    }
}
