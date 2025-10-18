using DevOpsApi.Models;

namespace DevOpsApi.Services;

/// <summary>
/// Interface del servicio de productos con lógica de negocio
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Obtiene todos los productos
    /// </summary>
    Task<IEnumerable<Product>> GetAllProductsAsync();

    /// <summary>
    /// Obtiene un producto por su ID
    /// </summary>
    Task<Product?> GetProductByIdAsync(int id);

    /// <summary>
    /// Obtiene un producto por su SKU
    /// </summary>
    Task<Product?> GetProductBySkuAsync(string sku);

    /// <summary>
    /// Obtiene productos por categoría
    /// </summary>
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);

    /// <summary>
    /// Obtiene solo productos activos
    /// </summary>
    Task<IEnumerable<Product>> GetActiveProductsAsync();

    /// <summary>
    /// Crea un nuevo producto con validaciones de negocio
    /// </summary>
    Task<Product> CreateProductAsync(Product product);

    /// <summary>
    /// Actualiza un producto existente con validaciones
    /// </summary>
    Task<Product> UpdateProductAsync(int id, Product product);

    /// <summary>
    /// Elimina un producto (soft delete - marca como inactivo)
    /// </summary>
    Task<bool> DeleteProductAsync(int id);

    /// <summary>
    /// Elimina permanentemente un producto
    /// </summary>
    Task<bool> HardDeleteProductAsync(int id);

    /// <summary>
    /// Verifica disponibilidad de stock
    /// </summary>
    Task<bool> IsStockAvailableAsync(int productId, int quantity);

    /// <summary>
    /// Reduce el stock de un producto
    /// </summary>
    Task<bool> ReduceStockAsync(int productId, int quantity);

    /// <summary>
    /// Incrementa el stock de un producto
    /// </summary>
    Task<bool> IncreaseStockAsync(int productId, int quantity);
}
