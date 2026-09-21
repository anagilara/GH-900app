namespace ghRepo.Models;

public class ProductAdminViewModel
{
    public List<Product> Products { get; set; } = [];

    public Product NewProduct { get; set; } = new();
}
