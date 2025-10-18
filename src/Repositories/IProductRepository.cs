using DevOpsApi.Models;

namespace DevOpsApi.Repositories;

/// <summary>
/// Interface del repositorio de productos para operaciones de acceso a datos
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Obtiene todos los productos
    /// </summary>
    Task<IEnumerable<Product>> GetAllAsync();

    /// <summary>
    /// Obtiene un producto por su ID
    /// </summary>
    Task<Product?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene un producto por su SKU
    /// </summary>
    Task<Product?> GetBySkuAsync(string sku);

    /// <summary>
    /// Obtiene productos por categoría
    /// </summary>
    Task<IEnumerable<Product>> GetByCategoryAsync(string category);

    /// <summary>
    /// Obtiene productos activos
    /// </summary>
    Task<IEnumerable<Product>> GetActiveProductsAsync();

    /// <summary>
    /// Crea un nuevo producto
    /// </summary>
    Task<Product> CreateAsync(Product product);

    /// <summary>
    /// Actualiza un producto existente
    /// </summary>
    Task<Product> UpdateAsync(Product product);

    /// <summary>
    /// Elimina un producto por su ID
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Verifica si existe un producto con el SKU especificado
    /// </summary>
    Task<bool> ExistsBySkuAsync(string sku, int? excludeId = null);

    /// <summary>
    /// Guarda los cambios en la base de datos
    /// </summary>
    Task<int> SaveChangesAsync();
}
