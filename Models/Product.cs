using System.ComponentModel.DataAnnotations;

namespace ghRepo.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El SKU es obligatorio")]
    [StringLength(40, ErrorMessage = "El SKU no puede exceder 40 caracteres")]
    public string Sku { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es obligatoria")]
    [StringLength(50, ErrorMessage = "La categoría no puede exceder 50 caracteres")]
    public string Category { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "99999999", ErrorMessage = "El precio debe ser mayor que cero")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
    public int Stock { get; set; }

    public bool IsActive { get; set; } = true;
}
