using System.Reflection;
using FluentAssertions;
using ghRepo.Controllers;
using ghRepo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ghRepo.Tests.Controllers;

public class ProductsControllerTests
{
    private static readonly FieldInfo ProductsField = typeof(ProductsController)
        .GetField("Products", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly FieldInfo NextIdField = typeof(ProductsController)
        .GetField("_nextId", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly FieldInfo SyncLockField = typeof(ProductsController)
        .GetField("SyncLock", BindingFlags.NonPublic | BindingFlags.Static)!;

    public ProductsControllerTests()
    {
        ResetState();
    }

    [Fact]
    public void Index_ShouldReturnViewWithProducts()
    {
        var controller = new ProductsController();

        var result = controller.Index();

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<ProductAdminViewModel>().Subject;
        model.Products.Should().HaveCount(3);
    }

    [Fact]
    public void SearchById_WhenExists_ShouldReturnProduct()
    {
        var product = ProductsController.SearchById(1);

        product.Should().NotBeNull();
        product.Sku.Should().Be("TECH-LP14");
    }

    [Fact]
    public void SearchByCategory_ShouldBeCaseInsensitive()
    {
        var products = ProductsController.SearchByCategory("tecnología");

        products.Should().ContainSingle();
        products[0].Name.Should().Be("Laptop Pro 14");
    }

    [Fact]
    public void Create_WhenModelStateInvalid_ShouldReturnIndexView()
    {
        var controller = new ProductsController();
        controller.ModelState.AddModelError("NewProduct.Name", "El nombre es obligatorio");

        var result = controller.Create(new ProductAdminViewModel());

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be("Index");
        var model = view.Model.Should().BeOfType<ProductAdminViewModel>().Subject;
        model.Products.Should().HaveCount(3);
    }

    [Fact]
    public void Create_WhenValid_ShouldAddProductAndRedirect()
    {
        var controller = BuildControllerWithTempData();
        var model = new ProductAdminViewModel
        {
            NewProduct = new Product
            {
                Sku = "TEST-NEW-01",
                Name = "Producto Nuevo",
                Category = "Pruebas",
                Price = 100m,
                Stock = 10,
                IsActive = true
            }
        };

        var result = controller.Create(model);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ProductsController.Index));
        ProductsController.SearchById(4).Should().NotBeNull();
        controller.TempData["SuccessMessage"].Should().Be("Producto agregado correctamente.");
    }

    [Fact]
    public void Delete_WhenExisting_ShouldRemoveProductAndRedirect()
    {
        var controller = BuildControllerWithTempData();

        var result = controller.Delete(2);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ProductsController.Index));
        ProductsController.SearchById(2).Should().BeNull();
    }

    [Fact]
    public void Delete_WhenIdDoesNotExist_ShouldNotChangeCount()
    {
        var controller = BuildControllerWithTempData();
        var before = GetProductCount();

        var result = controller.Delete(999);

        result.Should().BeOfType<RedirectToActionResult>();
        GetProductCount().Should().Be(before);
    }

    private static ProductsController BuildControllerWithTempData()
    {
        var controller = new ProductsController
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider())
        };

        return controller;
    }

    private static int GetProductCount()
    {
        var syncLock = SyncLockField.GetValue(null)!;
        var products = (List<Product>)ProductsField.GetValue(null)!;

        lock (syncLock)
        {
            return products.Count;
        }
    }

    private static void ResetState()
    {
        var syncLock = SyncLockField.GetValue(null)!;
        var products = (List<Product>)ProductsField.GetValue(null)!;

        lock (syncLock)
        {
            products.Clear();
            products.AddRange(
            [
                new Product { Id = 1, Sku = "TECH-LP14", Name = "Laptop Pro 14", Category = "Tecnología", Price = 1499.99m, Stock = 8, IsActive = true },
                new Product { Id = 2, Sku = "OFI-SILLA-ERG", Name = "Silla Ergonómica", Category = "Oficina", Price = 329.50m, Stock = 15, IsActive = true },
                new Product { Id = 3, Sku = "ACC-BOT-TERM", Name = "Botella Térmica", Category = "Accesorios", Price = 24.90m, Stock = 60, IsActive = true }
            ]);

            NextIdField.SetValue(null, 4);
        }
    }

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        private Dictionary<string, object?> _data = [];

        public IDictionary<string, object?> LoadTempData(HttpContext context)
        {
            return _data;
        }

        public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
        {
            _data = new Dictionary<string, object?>(values);
        }
    }
}