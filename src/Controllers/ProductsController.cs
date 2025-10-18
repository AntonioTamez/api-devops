using Microsoft.AspNetCore.Mvc;
using DevOpsApi.Services;
using DevOpsApi.Models;
using DevOpsApi.DTOs;

namespace DevOpsApi.Controllers;

/// <summary>
/// Controlador para gestionar productos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los productos con paginación y filtros opcionales
    /// </summary>
    /// <param name="pageNumber">Número de página (default: 1)</param>
    /// <param name="pageSize">Tamaño de página (default: 10, max: 100)</param>
    /// <param name="category">Filtrar por categoría (opcional)</param>
    /// <param name="activeOnly">Mostrar solo productos activos (default: false)</param>
    /// <returns>Lista paginada de productos</returns>
    /// <response code="200">Retorna la lista de productos</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAllProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? category = null,
        [FromQuery] bool activeOnly = false)
    {
        _logger.LogInformation("Getting products - Page: {PageNumber}, Size: {PageSize}, Category: {Category}, ActiveOnly: {ActiveOnly}",
            pageNumber, pageSize, category, activeOnly);

        // Validar parámetros de paginación
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        // Obtener productos según filtros
        IEnumerable<Product> products;
        
        if (!string.IsNullOrWhiteSpace(category))
        {
            products = await _productService.GetProductsByCategoryAsync(category);
        }
        else if (activeOnly)
        {
            products = await _productService.GetActiveProductsAsync();
        }
        else
        {
            products = await _productService.GetAllProductsAsync();
        }

        // Aplicar paginación
        var totalCount = products.Count();
        var items = products
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToDto(p))
            .ToList();

        var result = new PagedResult<ProductDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        _logger.LogInformation("Returned {Count} products out of {TotalCount}", items.Count, totalCount);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un producto por su ID
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <returns>Producto encontrado</returns>
    /// <response code="200">Retorna el producto</response>
    /// <response code="404">Producto no encontrado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProductById(int id)
    {
        _logger.LogInformation("Getting product by ID: {ProductId}", id);

        var product = await _productService.GetProductByIdAsync(id);
        
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", id);
            return NotFound(new { message = $"Producto con ID {id} no encontrado" });
        }

        return Ok(MapToDto(product));
    }

    /// <summary>
    /// Obtiene un producto por su SKU
    /// </summary>
    /// <param name="sku">SKU del producto</param>
    /// <returns>Producto encontrado</returns>
    /// <response code="200">Retorna el producto</response>
    /// <response code="404">Producto no encontrado</response>
    [HttpGet("sku/{sku}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProductBySku(string sku)
    {
        _logger.LogInformation("Getting product by SKU: {Sku}", sku);

        var product = await _productService.GetProductBySkuAsync(sku);
        
        if (product == null)
        {
            _logger.LogWarning("Product not found with SKU: {Sku}", sku);
            return NotFound(new { message = $"Producto con SKU '{sku}' no encontrado" });
        }

        return Ok(MapToDto(product));
    }

    /// <summary>
    /// Crea un nuevo producto
    /// </summary>
    /// <param name="createDto">Datos del producto a crear</param>
    /// <returns>Producto creado</returns>
    /// <response code="201">Producto creado exitosamente</response>
    /// <response code="400">Datos inválidos o SKU duplicado</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto createDto)
    {
        _logger.LogInformation("Creating new product: {ProductName}", createDto.Name);

        try
        {
            var product = new Product
            {
                Name = createDto.Name,
                Description = createDto.Description,
                Sku = createDto.Sku,
                Price = createDto.Price,
                Stock = createDto.Stock,
                Category = createDto.Category,
                IsActive = createDto.IsActive
            };

            var createdProduct = await _productService.CreateProductAsync(product);
            var productDto = MapToDto(createdProduct);

            _logger.LogInformation("Product created successfully with ID: {ProductId}", createdProduct.Id);
            
            return CreatedAtAction(
                nameof(GetProductById),
                new { id = createdProduct.Id },
                productDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create product: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid argument for product creation: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza un producto existente
    /// </summary>
    /// <param name="id">ID del producto a actualizar</param>
    /// <param name="updateDto">Datos actualizados del producto</param>
    /// <returns>Producto actualizado</returns>
    /// <response code="200">Producto actualizado exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="404">Producto no encontrado</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductDto updateDto)
    {
        _logger.LogInformation("Updating product ID: {ProductId}", id);

        try
        {
            var product = new Product
            {
                Name = updateDto.Name,
                Description = updateDto.Description,
                Sku = updateDto.Sku,
                Price = updateDto.Price,
                Stock = updateDto.Stock,
                Category = updateDto.Category,
                IsActive = updateDto.IsActive
            };

            var updatedProduct = await _productService.UpdateProductAsync(id, product);
            var productDto = MapToDto(updatedProduct);

            _logger.LogInformation("Product updated successfully: {ProductId}", id);
            return Ok(productDto);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Product not found for update: {ProductId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update product: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid argument for product update: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Elimina un producto (soft delete - marca como inactivo)
    /// </summary>
    /// <param name="id">ID del producto a eliminar</param>
    /// <returns>Confirmación de eliminación</returns>
    /// <response code="204">Producto eliminado exitosamente</response>
    /// <response code="404">Producto no encontrado</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        _logger.LogInformation("Deleting product ID: {ProductId}", id);

        var result = await _productService.DeleteProductAsync(id);
        
        if (!result)
        {
            _logger.LogWarning("Product not found for deletion: {ProductId}", id);
            return NotFound(new { message = $"Producto con ID {id} no encontrado" });
        }

        _logger.LogInformation("Product deleted successfully: {ProductId}", id);
        return NoContent();
    }

    /// <summary>
    /// Elimina permanentemente un producto (hard delete)
    /// </summary>
    /// <param name="id">ID del producto a eliminar</param>
    /// <returns>Confirmación de eliminación</returns>
    /// <response code="204">Producto eliminado permanentemente</response>
    /// <response code="404">Producto no encontrado</response>
    [HttpDelete("{id}/permanent")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> HardDeleteProduct(int id)
    {
        _logger.LogInformation("Hard deleting product ID: {ProductId}", id);

        var result = await _productService.HardDeleteProductAsync(id);
        
        if (!result)
        {
            _logger.LogWarning("Product not found for hard deletion: {ProductId}", id);
            return NotFound(new { message = $"Producto con ID {id} no encontrado" });
        }

        _logger.LogInformation("Product hard deleted successfully: {ProductId}", id);
        return NoContent();
    }

    /// <summary>
    /// Verifica disponibilidad de stock
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <param name="quantity">Cantidad requerida</param>
    /// <returns>Disponibilidad de stock</returns>
    /// <response code="200">Retorna disponibilidad</response>
    /// <response code="404">Producto no encontrado</response>
    [HttpGet("{id}/stock/check")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CheckStock(int id, [FromQuery] int quantity = 1)
    {
        _logger.LogInformation("Checking stock for product {ProductId}, quantity: {Quantity}", id, quantity);

        var isAvailable = await _productService.IsStockAvailableAsync(id, quantity);
        
        if (!isAvailable && await _productService.GetProductByIdAsync(id) == null)
        {
            return NotFound(new { message = $"Producto con ID {id} no encontrado" });
        }

        return Ok(new { productId = id, quantity, isAvailable });
    }

    /// <summary>
    /// Reduce el stock de un producto
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <param name="quantity">Cantidad a reducir</param>
    /// <returns>Confirmación de reducción</returns>
    /// <response code="200">Stock reducido exitosamente</response>
    /// <response code="400">Stock insuficiente</response>
    /// <response code="404">Producto no encontrado</response>
    [HttpPost("{id}/stock/reduce")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReduceStock(int id, [FromQuery] int quantity)
    {
        _logger.LogInformation("Reducing stock for product {ProductId}, quantity: {Quantity}", id, quantity);

        try
        {
            var result = await _productService.ReduceStockAsync(id, quantity);
            
            if (!result)
            {
                return NotFound(new { message = $"Producto con ID {id} no encontrado" });
            }

            return Ok(new { message = "Stock reducido exitosamente", productId = id, quantityReduced = quantity });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to reduce stock: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid argument for stock reduction: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Aumenta el stock de un producto
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <param name="quantity">Cantidad a aumentar</param>
    /// <returns>Confirmación de aumento</returns>
    /// <response code="200">Stock aumentado exitosamente</response>
    /// <response code="400">Cantidad inválida</response>
    /// <response code="404">Producto no encontrado</response>
    [HttpPost("{id}/stock/increase")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IncreaseStock(int id, [FromQuery] int quantity)
    {
        _logger.LogInformation("Increasing stock for product {ProductId}, quantity: {Quantity}", id, quantity);

        try
        {
            var result = await _productService.IncreaseStockAsync(id, quantity);
            
            if (!result)
            {
                return NotFound(new { message = $"Producto con ID {id} no encontrado" });
            }

            return Ok(new { message = "Stock aumentado exitosamente", productId = id, quantityIncreased = quantity });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid argument for stock increase: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    // Helper method para mapear Product a ProductDto
    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Sku = product.Sku,
            Price = product.Price,
            Stock = product.Stock,
            Category = product.Category,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
