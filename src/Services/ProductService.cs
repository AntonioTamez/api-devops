using DevOpsApi.Models;
using DevOpsApi.Repositories;

namespace DevOpsApi.Services;

/// <summary>
/// Implementación del servicio de productos con lógica de negocio
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository repository, ILogger<ProductService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        _logger.LogInformation("Getting all products");
        return await _repository.GetAllAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        _logger.LogInformation("Getting product by ID: {ProductId}", id);
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Product?> GetProductBySkuAsync(string sku)
    {
        _logger.LogInformation("Getting product by SKU: {Sku}", sku);
        return await _repository.GetBySkuAsync(sku);
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
    {
        _logger.LogInformation("Getting products by category: {Category}", category);
        return await _repository.GetByCategoryAsync(category);
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        _logger.LogInformation("Getting active products");
        return await _repository.GetActiveProductsAsync();
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        _logger.LogInformation("Creating new product: {ProductName}", product.Name);

        // Validación de negocio: SKU debe ser único
        if (await _repository.ExistsBySkuAsync(product.Sku))
        {
            _logger.LogWarning("Attempt to create product with duplicate SKU: {Sku}", product.Sku);
            throw new InvalidOperationException($"Ya existe un producto con el SKU '{product.Sku}'");
        }

        // Validación de negocio: Precio debe ser mayor a 0
        if (product.Price <= 0)
        {
            throw new ArgumentException("El precio debe ser mayor a 0", nameof(product.Price));
        }

        // Validación de negocio: Stock no puede ser negativo
        if (product.Stock < 0)
        {
            throw new ArgumentException("El stock no puede ser negativo", nameof(product.Stock));
        }

        // Normalizar nombre y SKU
        product.Name = product.Name.Trim();
        product.Sku = product.Sku.Trim().ToUpperInvariant();

        var createdProduct = await _repository.CreateAsync(product);
        _logger.LogInformation("Product created successfully with ID: {ProductId}", createdProduct.Id);

        return createdProduct;
    }

    public async Task<Product> UpdateProductAsync(int id, Product product)
    {
        _logger.LogInformation("Updating product ID: {ProductId}", id);

        var existingProduct = await _repository.GetByIdAsync(id);
        if (existingProduct == null)
        {
            _logger.LogWarning("Product not found for update: {ProductId}", id);
            throw new KeyNotFoundException($"Producto con ID {id} no encontrado");
        }

        // Validación de negocio: SKU debe ser único (excluyendo el producto actual)
        if (await _repository.ExistsBySkuAsync(product.Sku, id))
        {
            _logger.LogWarning("Attempt to update product with duplicate SKU: {Sku}", product.Sku);
            throw new InvalidOperationException($"Ya existe otro producto con el SKU '{product.Sku}'");
        }

        // Validación de negocio: Precio debe ser mayor a 0
        if (product.Price <= 0)
        {
            throw new ArgumentException("El precio debe ser mayor a 0", nameof(product.Price));
        }

        // Validación de negocio: Stock no puede ser negativo
        if (product.Stock < 0)
        {
            throw new ArgumentException("El stock no puede ser negativo", nameof(product.Stock));
        }

        // Actualizar propiedades
        existingProduct.Name = product.Name.Trim();
        existingProduct.Description = product.Description?.Trim();
        existingProduct.Sku = product.Sku.Trim().ToUpperInvariant();
        existingProduct.Price = product.Price;
        existingProduct.Stock = product.Stock;
        existingProduct.Category = product.Category?.Trim();
        existingProduct.IsActive = product.IsActive;

        var updatedProduct = await _repository.UpdateAsync(existingProduct);
        _logger.LogInformation("Product updated successfully: {ProductId}", id);

        return updatedProduct;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        _logger.LogInformation("Soft deleting product ID: {ProductId}", id);

        var product = await _repository.GetByIdAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Product not found for soft delete: {ProductId}", id);
            return false;
        }

        // Soft delete: marcar como inactivo
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(product);

        _logger.LogInformation("Product soft deleted successfully: {ProductId}", id);
        return true;
    }

    public async Task<bool> HardDeleteProductAsync(int id)
    {
        _logger.LogInformation("Hard deleting product ID: {ProductId}", id);

        var result = await _repository.DeleteAsync(id);
        if (result)
        {
            _logger.LogInformation("Product hard deleted successfully: {ProductId}", id);
        }
        else
        {
            _logger.LogWarning("Product not found for hard delete: {ProductId}", id);
        }

        return result;
    }

    public async Task<bool> IsStockAvailableAsync(int productId, int quantity)
    {
        _logger.LogInformation("Checking stock availability for product {ProductId}, quantity: {Quantity}", productId, quantity);

        var product = await _repository.GetByIdAsync(productId);
        if (product == null)
        {
            _logger.LogWarning("Product not found for stock check: {ProductId}", productId);
            return false;
        }

        return product.Stock >= quantity;
    }

    public async Task<bool> ReduceStockAsync(int productId, int quantity)
    {
        _logger.LogInformation("Reducing stock for product {ProductId}, quantity: {Quantity}", productId, quantity);

        if (quantity <= 0)
        {
            throw new ArgumentException("La cantidad debe ser mayor a 0", nameof(quantity));
        }

        var product = await _repository.GetByIdAsync(productId);
        if (product == null)
        {
            _logger.LogWarning("Product not found for stock reduction: {ProductId}", productId);
            return false;
        }

        if (product.Stock < quantity)
        {
            _logger.LogWarning("Insufficient stock for product {ProductId}. Available: {Available}, Requested: {Requested}",
                productId, product.Stock, quantity);
            throw new InvalidOperationException($"Stock insuficiente. Disponible: {product.Stock}, Solicitado: {quantity}");
        }

        product.Stock -= quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(product);

        _logger.LogInformation("Stock reduced successfully for product {ProductId}. New stock: {NewStock}", productId, product.Stock);
        return true;
    }

    public async Task<bool> IncreaseStockAsync(int productId, int quantity)
    {
        _logger.LogInformation("Increasing stock for product {ProductId}, quantity: {Quantity}", productId, quantity);

        if (quantity <= 0)
        {
            throw new ArgumentException("La cantidad debe ser mayor a 0", nameof(quantity));
        }

        var product = await _repository.GetByIdAsync(productId);
        if (product == null)
        {
            _logger.LogWarning("Product not found for stock increase: {ProductId}", productId);
            return false;
        }

        product.Stock += quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(product);

        _logger.LogInformation("Stock increased successfully for product {ProductId}. New stock: {NewStock}", productId, product.Stock);
        return true;
    }
}
