using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using ghRepo.Models;

namespace ghRepo.Tests.Models;

public class ProductTests
{
    [Fact]
    public void IsActive_DefaultValue_ShouldBeTrue()
    {
        var product = new Product();

        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Sku_WhenMissing_ShouldFailValidation()
    {
        var product = BuildValidProduct();
        product.Sku = string.Empty;

        var errors = ValidateModel(product);

        errors.Should().Contain(e => e.MemberNames.Contains(nameof(Product.Sku)));
    }

    [Fact]
    public void Price_WhenLessOrEqualToZero_ShouldFailValidation()
    {
        var product = BuildValidProduct();
        product.Price = 0m;

        var errors = ValidateModel(product);

        errors.Should().Contain(e => e.MemberNames.Contains(nameof(Product.Price)));
    }

    [Fact]
    public void Stock_WhenNegative_ShouldFailValidation()
    {
        var product = BuildValidProduct();
        product.Stock = -1;

        var errors = ValidateModel(product);

        errors.Should().Contain(e => e.MemberNames.Contains(nameof(Product.Stock)));
    }

    private static Product BuildValidProduct()
    {
        return new Product
        {
            Sku = "TEST-001",
            Name = "Producto de prueba",
            Category = "Categoría",
            Price = 10.50m,
            Stock = 5,
            IsActive = true
        };
    }

    private static List<ValidationResult> ValidateModel(Product product)
    {
        var context = new ValidationContext(product);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(product, context, results, validateAllProperties: true);
        return results;
    }
}