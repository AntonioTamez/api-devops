using DevOpsApi.Controllers;
using DevOpsApi.DTOs;
using DevOpsApi.Models;
using DevOpsApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DevOpsApi.UnitTests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _serviceMock;
    private readonly Mock<ILogger<ProductsController>> _loggerMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _serviceMock = new Mock<IProductService>();
        _loggerMock = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(_serviceMock.Object, _loggerMock.Object);
    }

    #region GetAllProducts Tests

    [Fact]
    public async Task GetAllProducts_WithDefaultParameters_ShouldReturnPagedResult()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Sku = "SKU001", Price = 10M, Stock = 100, IsActive = true },
            new Product { Id = 2, Name = "Product 2", Sku = "SKU002", Price = 20M, Stock = 50, IsActive = true }
        };
        _serviceMock.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(products);

        // Act
        var result = await _controller.GetAllProducts();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var pagedResult = okResult.Value.Should().BeOfType<PagedResult<ProductDto>>().Subject;
        pagedResult.Items.Should().HaveCount(2);
        pagedResult.TotalCount.Should().Be(2);
        pagedResult.PageNumber.Should().Be(1);
        pagedResult.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAllProducts_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var products = Enumerable.Range(1, 25).Select(i => new Product
        {
            Id = i,
            Name = $"Product {i}",
            Sku = $"SKU{i:000}",
            Price = 10M * i,
            Stock = 100,
            IsActive = true
        }).ToList();
        _serviceMock.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(products);

        // Act
        var result = await _controller.GetAllProducts(pageNumber: 2, pageSize: 10);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var pagedResult = okResult.Value.Should().BeOfType<PagedResult<ProductDto>>().Subject;
        pagedResult.Items.Should().HaveCount(10);
        pagedResult.TotalCount.Should().Be(25);
        pagedResult.PageNumber.Should().Be(2);
        pagedResult.Items.First().Id.Should().Be(11);
    }

    [Fact]
    public async Task GetAllProducts_WithCategory_ShouldFilterByCategory()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Sku = "SKU001", Category = "Electronics", Price = 10M, Stock = 100 },
            new Product { Id = 2, Name = "Product 2", Sku = "SKU002", Category = "Electronics", Price = 20M, Stock = 50 }
        };
        _serviceMock.Setup(s => s.GetProductsByCategoryAsync("Electronics")).ReturnsAsync(products);

        // Act
        var result = await _controller.GetAllProducts(category: "Electronics");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var pagedResult = okResult.Value.Should().BeOfType<PagedResult<ProductDto>>().Subject;
        pagedResult.Items.Should().HaveCount(2);
        pagedResult.Items.Should().OnlyContain(p => p.Category == "Electronics");
    }

    [Fact]
    public async Task GetAllProducts_WithActiveOnly_ShouldReturnOnlyActiveProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Sku = "SKU001", Price = 10M, Stock = 100, IsActive = true },
            new Product { Id = 2, Name = "Product 2", Sku = "SKU002", Price = 20M, Stock = 50, IsActive = true }
        };
        _serviceMock.Setup(s => s.GetActiveProductsAsync()).ReturnsAsync(products);

        // Act
        var result = await _controller.GetAllProducts(activeOnly: true);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var pagedResult = okResult.Value.Should().BeOfType<PagedResult<ProductDto>>().Subject;
        pagedResult.Items.Should().OnlyContain(p => p.IsActive);
    }

    #endregion

    #region GetProductById Tests

    [Fact]
    public async Task GetProductById_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", Sku = "SKU001", Price = 10M, Stock = 100 };
        _serviceMock.Setup(s => s.GetProductByIdAsync(1)).ReturnsAsync(product);

        // Act
        var result = await _controller.GetProductById(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var productDto = okResult.Value.Should().BeOfType<ProductDto>().Subject;
        productDto.Id.Should().Be(1);
        productDto.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetProductById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetProductByIdAsync(999)).ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.GetProductById(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region GetProductBySku Tests

    [Fact]
    public async Task GetProductBySku_WithValidSku_ShouldReturnProduct()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", Sku = "SKU001", Price = 10M, Stock = 100 };
        _serviceMock.Setup(s => s.GetProductBySkuAsync("SKU001")).ReturnsAsync(product);

        // Act
        var result = await _controller.GetProductBySku("SKU001");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var productDto = okResult.Value.Should().BeOfType<ProductDto>().Subject;
        productDto.Sku.Should().Be("SKU001");
    }

    [Fact]
    public async Task GetProductBySku_WithInvalidSku_ShouldReturnNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetProductBySkuAsync("INVALID")).ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.GetProductBySku("INVALID");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region CreateProduct Tests

    [Fact]
    public async Task CreateProduct_WithValidDto_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "New Product",
            Description = "Description",
            Sku = "SKU123",
            Price = 15.99M,
            Stock = 50,
            Category = "Test",
            IsActive = true
        };

        var createdProduct = new Product
        {
            Id = 1,
            Name = createDto.Name,
            Description = createDto.Description,
            Sku = createDto.Sku,
            Price = createDto.Price,
            Stock = createDto.Stock,
            Category = createDto.Category,
            IsActive = createDto.IsActive
        };

        _serviceMock.Setup(s => s.CreateProductAsync(It.IsAny<Product>())).ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.CreateProduct(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ProductsController.GetProductById));
        var productDto = createdResult.Value.Should().BeOfType<ProductDto>().Subject;
        productDto.Name.Should().Be("New Product");
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSku_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Product",
            Sku = "SKU001",
            Price = 10M,
            Stock = 10
        };

        _serviceMock.Setup(s => s.CreateProductAsync(It.IsAny<Product>()))
            .ThrowsAsync(new InvalidOperationException("Ya existe un producto con el SKU 'SKU001'"));

        // Act
        var result = await _controller.CreateProduct(createDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateProduct_WithInvalidPrice_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Product",
            Sku = "SKU001",
            Price = 0M,
            Stock = 10
        };

        _serviceMock.Setup(s => s.CreateProductAsync(It.IsAny<Product>()))
            .ThrowsAsync(new ArgumentException("El precio debe ser mayor a 0"));

        // Act
        var result = await _controller.CreateProduct(createDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region UpdateProduct Tests

    [Fact]
    public async Task UpdateProduct_WithValidId_ShouldReturnOkWithUpdatedProduct()
    {
        // Arrange
        var updateDto = new UpdateProductDto
        {
            Name = "Updated Product",
            Sku = "SKU001",
            Price = 25.99M,
            Stock = 75,
            IsActive = true
        };

        var updatedProduct = new Product
        {
            Id = 1,
            Name = updateDto.Name,
            Sku = updateDto.Sku,
            Price = updateDto.Price,
            Stock = updateDto.Stock,
            IsActive = updateDto.IsActive
        };

        _serviceMock.Setup(s => s.UpdateProductAsync(1, It.IsAny<Product>())).ReturnsAsync(updatedProduct);

        // Act
        var result = await _controller.UpdateProduct(1, updateDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var productDto = okResult.Value.Should().BeOfType<ProductDto>().Subject;
        productDto.Name.Should().Be("Updated Product");
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var updateDto = new UpdateProductDto { Name = "Test", Sku = "SKU001", Price = 10M, Stock = 10 };
        _serviceMock.Setup(s => s.UpdateProductAsync(999, It.IsAny<Product>()))
            .ThrowsAsync(new KeyNotFoundException("Producto con ID 999 no encontrado"));

        // Act
        var result = await _controller.UpdateProduct(999, updateDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region DeleteProduct Tests

    [Fact]
    public async Task DeleteProduct_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteProductAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteProduct(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteProduct_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteProductAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteProduct(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task HardDeleteProduct_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        _serviceMock.Setup(s => s.HardDeleteProductAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.HardDeleteProduct(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task HardDeleteProduct_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.HardDeleteProductAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _controller.HardDeleteProduct(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Stock Management Tests

    [Fact]
    public async Task CheckStock_WithSufficientStock_ShouldReturnAvailable()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Product", Sku = "SKU001", Price = 10M, Stock = 100 };
        _serviceMock.Setup(s => s.IsStockAvailableAsync(1, 50)).ReturnsAsync(true);
        _serviceMock.Setup(s => s.GetProductByIdAsync(1)).ReturnsAsync(product);

        // Act
        var result = await _controller.CheckStock(1, 50);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckStock_WithProductNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.IsStockAvailableAsync(999, 50)).ReturnsAsync(false);
        _serviceMock.Setup(s => s.GetProductByIdAsync(999)).ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.CheckStock(999, 50);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ReduceStock_WithValidQuantity_ShouldReturnOk()
    {
        // Arrange
        _serviceMock.Setup(s => s.ReduceStockAsync(1, 30)).ReturnsAsync(true);

        // Act
        var result = await _controller.ReduceStock(1, 30);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ReduceStock_WithInsufficientStock_ShouldReturnBadRequest()
    {
        // Arrange
        _serviceMock.Setup(s => s.ReduceStockAsync(1, 200))
            .ThrowsAsync(new InvalidOperationException("Stock insuficiente"));

        // Act
        var result = await _controller.ReduceStock(1, 200);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ReduceStock_WithProductNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.ReduceStockAsync(999, 30)).ReturnsAsync(false);

        // Act
        var result = await _controller.ReduceStock(999, 30);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task IncreaseStock_WithValidQuantity_ShouldReturnOk()
    {
        // Arrange
        _serviceMock.Setup(s => s.IncreaseStockAsync(1, 50)).ReturnsAsync(true);

        // Act
        var result = await _controller.IncreaseStock(1, 50);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task IncreaseStock_WithInvalidQuantity_ShouldReturnBadRequest()
    {
        // Arrange
        _serviceMock.Setup(s => s.IncreaseStockAsync(1, 0))
            .ThrowsAsync(new ArgumentException("La cantidad debe ser mayor a 0"));

        // Act
        var result = await _controller.IncreaseStock(1, 0);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task IncreaseStock_WithProductNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.IncreaseStockAsync(999, 50)).ReturnsAsync(false);

        // Act
        var result = await _controller.IncreaseStock(999, 50);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}
