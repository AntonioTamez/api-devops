using System.ComponentModel.DataAnnotations;

namespace DevOpsApi.DTOs;

/// <summary>
/// DTO para actualizar un producto existente
/// </summary>
public class UpdateProductDto
{
    /// <summary>
    /// Nombre del producto
    /// </summary>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del producto
    /// </summary>
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Description { get; set; }

    /// <summary>
    /// Código SKU único del producto
    /// </summary>
    [Required(ErrorMessage = "El SKU es obligatorio")]
    [StringLength(50, ErrorMessage = "El SKU no puede exceder 50 caracteres")]
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Precio del producto
    /// </summary>
    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe estar entre 0.01 y 999,999.99")]
    public decimal Price { get; set; }

    /// <summary>
    /// Cantidad en stock
    /// </summary>
    [Required(ErrorMessage = "El stock es obligatorio")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
    public int Stock { get; set; }

    /// <summary>
    /// Categoría del producto
    /// </summary>
    [StringLength(50, ErrorMessage = "La categoría no puede exceder 50 caracteres")]
    public string? Category { get; set; }

    /// <summary>
    /// Indica si el producto está activo
    /// </summary>
    public bool IsActive { get; set; }
}
