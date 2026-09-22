using ghRepo.Models;
using Microsoft.AspNetCore.Mvc;
//test rebase and merge 1
//test rebase and merge 2
//test squash and merge 1
//test squash and merge 2
//test merge 1
//test merge 2
namespace ghRepo.Controllers;
public class ProductsController : Controller
{
    private static readonly object SyncLock = new();
    private static readonly List<Product> Products =
    [
        new Product { Id = 1, Sku = "TECH-LP14", Name = "Laptop Pro 14", Category = "Tecnología", Price = 1499.99m, Stock = 8, IsActive = true },
        new Product { Id = 2, Sku = "OFI-SILLA-ERG", Name = "Silla Ergonómica", Category = "Oficina", Price = 329.50m, Stock = 15, IsActive = true },
        new Product { Id = 3, Sku = "ACC-BOT-TERM", Name = "Botella Térmica", Category = "Accesorios", Price = 24.90m, Stock = 60, IsActive = true }
    ];

    private static int _nextId = 4;

    public IActionResult Index()
    {
        return View(BuildViewModel());
    }

    public static Product SearchById(int id)
    {
        lock (SyncLock)
        {
            return Products.FirstOrDefault(p => p.Id == id);
        }
    }

    public static List<Product> SearchByCategory(string category)
    {
        lock (SyncLock)
        {
            return Products
                .Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ProductAdminViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Products = GetProductsSnapshot();
            return View("Index", model);
        }

        var product = model.NewProduct;

        lock (SyncLock)
        {
            product.Id = _nextId;
            _nextId++;
            Products.Add(product);
        }

        TempData["SuccessMessage"] = "Producto agregado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        lock (SyncLock)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            if (product is not null)
            {
                Products.Remove(product);
            }
        }

        TempData["SuccessMessage"] = "Producto eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private static ProductAdminViewModel BuildViewModel()
    {
        return new ProductAdminViewModel
        {
            Products = GetProductsSnapshot()
        };
    }

    private static List<Product> GetProductsSnapshot()
    {
        lock (SyncLock)
        {
            return Products
                .OrderBy(p => p.Name)
                .Select(p => new Product
                {
                    Id = p.Id,
                    Sku = p.Sku,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price,
                    Stock = p.Stock,
                    IsActive = p.IsActive
                })
                .ToList();
        }
    }
}
