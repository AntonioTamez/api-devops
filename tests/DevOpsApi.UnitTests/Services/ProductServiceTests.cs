using DevOpsApi.Models;
using DevOpsApi.Repositories;
using DevOpsApi.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DevOpsApi.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock&lt;IProductRepository&gt; _repositoryMock;
    private readonly Mock&lt;ILogger&lt;ProductService&gt;&gt; _loggerMock;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock&lt;IProductRepository&gt;();
        _loggerMock = new Mock&lt;ILogger&lt;ProductService&gt;&gt;();
        _service = new ProductService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllProductsAsync_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new List&lt;Product&gt;
        {
            new Product { Id = 1, Name = "Product 1", Sku = "SKU001", Price = 10M, Stock = 100, IsActive = true },
            new Product { Id = 2, Name = "Product 2", Sku = "SKU002", Price = 20M, Stock = 50, IsActive = true }
        };
        _repositoryMock.Setup(r =&gt; r.GetAllAsync()).ReturnsAsync(products);

        // Act
        var result = await _service.GetAllProductsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        _repositoryMock.Verify(r =&gt; r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", Sku = "SKU001", Price = 10M, Stock = 100 };
        _repositoryMock.Setup(r =&gt; r.GetByIdAsync(1)).ReturnsAsync(product);

        // Act
        var result = await _service.GetProductByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetProductByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _repositoryMock.Setup(r =&gt; r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        // Act
        var result = await _service.GetProductByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProductBySkuAsync_WithValidSku_ShouldReturnProduct()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", Sku = "SKU001", Price = 10M, Stock = 100 };
        _repositoryMock.Setup(r =&gt; r.GetBySkuAsync("SKU001")).ReturnsAsync(product);

        // Act
        var result = await _service.GetProductBySkuAsync("SKU001");

        // Assert
        result.Should().NotBeNull();
        result!.Sku.Should().Be("SKU001");
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_ShouldReturnProductsInCategory()
    {
        // Arrange
        var products = new List&lt;Product&gt;
        {
            new Product { Id = 1, Name = "Product 1", Sku = "SKU001", Category = "Electronics", Price = 10M, Stock = 100 },
            new Product { Id = 2, Name = "Product 2", Sku = "SKU002", Category = "Electronics", Price = 20M, Stock = 50 }
        };
        _repositoryMock.Setup(r =&gt; r.GetByCategoryAsync("Electronics")).ReturnsAsync(products);

        // Act
        var result = await _service.GetProductsByCategoryAsync("Electronics");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p =&gt; p.Category == "Electronics");
    }

    [Fact]
    public async Task GetActiveProductsAsync_ShouldReturnOnlyActiveProducts()
    {
        // Arrange
        var products = new List&lt;Product&gt;
        {
            new Product { Id = 1, Name = "Product 1", Sku = "SKU001", Price = 10M, Stock = 100, IsActive = true },
            new Product { Id = 2, Name = "Product 2", Sku = "SKU002", Price = 20M, Stock = 50, IsActive = true }
        };
        _repositoryMock.Setup(r =&gt; r.GetActiveProductsAsync()).ReturnsAsync(products);

        // Act
        var result = await _service.GetActiveProductsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p =&gt; p.IsActive);
    }

    [Fact]
    public async Task CreateProductAsync_WithValidProduct_ShouldCreateAndReturnProduct()
    {
        // Arrange
        var newProduct = new Product
        {
            Name = "New Product",
            Sku = "SKU123",
            Description = "Test description",
            Price = 15.99M,
            Stock = 75,
            Category = "Test"
        };

        _repositoryMock.Setup(r =&gt; r.ExistsBySkuAsync(It.IsAny&lt;string&gt;())).ReturnsAsync(false);
        _repositoryMock.Setup(r =&gt; r.CreateAsync(It.IsAny&lt;Product&gt;())).ReturnsAsync(
            (Product p) =&gt; { p.Id = 1; return p; });

        // Act
        var result = await _service.CreateProductAsync(newProduct);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Product");
        result.Sku.Should().Be("SKU123");
    }

    [Fact]
    public async Task CreateProductAsync_WithDuplicateSku_ShouldThrowException()
    {
        // Arrange
        var product = new Product { Name = "Product", Sku = "SKU001", Price = 10M, Stock = 10 };
        _repositoryMock.Setup(r =&gt; r.ExistsBySkuAsync("SKU001")).ReturnsAsync(true);

        // Act &amp; Assert
        await Assert.ThrowsAsync&lt;InvalidOperationException&gt;(() =&gt; _service.CreateProductAsync(product));
    }

    [Fact]
    public async Task CreateProductAsync_WithInvalidPrice_ShouldThrowException()
    {
        // Arrange
        var product = new Product { Name = "Product", Sku = "SKU001", Price = 0M, Stock = 10 };
        _repositoryMock.Setup(r =&gt; r.ExistsBySkuAsync(It.IsAny&lt;string&gt;())).ReturnsAsync(false);

        // Act &amp; Assert
        await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; _service.CreateProductAsync(product));
    }

    [Fact]
    public async Task CreateProductAsync_WithNegativeStock_ShouldThrowException()
    {
        // Arrange
        var product = new Product { Name = "Product", Sku = "SKU001", Price = 10M, Stock = -5 };
        _repositoryMock.Setup(r =&gt; r.ExistsBySkuAsync(It.IsAny&lt;string&gt;())).ReturnsAsync(false);

        // Act &amp; Assert
        await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; _service.CreateProductAsync(product));
    }

    [Fact]
    public async Task UpdateProductAsync_WithValidId_ShouldUpdateProduct()
    {
        // Arrange
        var existingProduct = new Product { Id = 1, Name = "Old Name", Sku = "SKU001", Price = 10M, Stock = 100 };
        var updateProduct = new Product { Name = "Updated Name", Sku = "SKU001", Price = 25.99M, Stock = 75 };

        _repositoryMock.Setup(r =&gt; r.GetByIdAsync(1)).ReturnsAsync(existingProduct);
        _repositoryMock.Setup(r =&gt; r.ExistsBySkuAsync("SKU001", 1)).ReturnsAsync(false);
        _repositoryMock.Setup(r =&gt; r.UpdateAsync(It.IsAny&lt;Product&gt;())).ReturnsAsync(
            (Product p) =&gt; p);

        // Act
        var result = await _service.UpdateProductAsync(1, updateProduct);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Price.Should().Be(25.99M);
    }

    [Fact]
    public async Task UpdateProductAsync_WithInvalidId_ShouldThrowException()
    {
        // Arrange
        var product = new Product { Name = "Product", Sku = "SKU001", Price = 10M, Stock = 10 };
        _repositoryMock.Setup(r =&gt; r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        // Act &amp; Assert
        await Assert.ThrowsAsync&lt;KeyNotFoundException&gt;(() =&gt; _service.UpdateProductAsync(999, product));
    }

    [Fact]
    public async Task DeleteProductAsync_WithValidId_ShouldSoftDeleteProduct()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Product", Sku = "SKU001", Price = 10M, Stock = 10, IsActive = true };
        _repositoryMock.Setup(r =&gt; r.GetByIdAsync(1)).ReturnsAsync(product);
        _repositoryMock.Setup(r =&gt; r.UpdateAsync(It.IsAny&lt;Product&gt;())).ReturnsAsync(product);

        // Act
        var result = await _service.DeleteProductAsync(1);

        // Assert
        result.Should().BeTrue();
        product.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteProductAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        _repositoryMock.Setup(r =&gt; r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        // Act
        var result = await _service.DeleteProductAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HardDeleteProductAsync_WithValidId_ShouldDeletePermanently()
    {
        // Arrange
        _repositoryMock.Setup(r =&gt; r.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _service.HardDeleteProductAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsStockAvailableAsync_WithSufficientStock_ShouldReturnTrue()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Product", Sku = "SKU001", Price = 10M, Stock = 100 };
        _repositoryMock.Setup(r =&gt; r.GetByIdAsync(1)).ReturnsAsync(product);

        // Act
        var result = await _service.IsStockAvailableAsync(1, 50);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsStockAvailableAsync_WithInsufficientStock_ShouldReturnFalse()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Product", Sku = "SKU001", Price = 10M, Stock = 10 };
        _repositoryMock.Setup(r =&gt; r.GetByIdAsync(1)).ReturnsAsync(product);

        // Act
        var result = await _service.IsStockAvailableAsync(1, 50);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReduceStockAsync_WithValidQuantity_ShouldReduceStock()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Product", Sku = "SKU001", Price = 10M, Stock = 100 };
        _repositoryMock.Setup(r =&gt; r.GetByIdAsync(1)).ReturnsAsync(product);
        _repositoryMock.Setup(r =&gt; r.UpdateAsync(It.IsAny&lt;Product&gt;())).ReturnsAsync(product);

        // Act
        var result = await _service.ReduceStockAsync(1, 30);

        // Assert
        result.Should().BeTrue();
        product.Stock.Should().Be(70);
    }

    [Fact]
    public async Task ReduceStockAsync_WithInsufficientStock_ShouldThrowException()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Product", Sku = "SKU001", Price = 10M, Stock = 10 };
        _repositoryMock.Setup(r =&gt; r.GetByIdAsync(1)).ReturnsAsync(product);

        // Act &amp; Assert
        await Assert.ThrowsAsync&lt;InvalidOperationException&gt;(() =&gt; _service.ReduceStockAsync(1, 50));
    }

    [Fact]
    public async Task IncreaseStockAsync_WithValidQuantity_ShouldIncreaseStock()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Product", Sku = "SKU001", Price = 10M, Stock = 100 };
        _repositoryMock.Setup(r =&gt; r.GetByIdAsync(1)).ReturnsAsync(product);
        _repositoryMock.Setup(r =&gt; r.UpdateAsync(It.IsAny&lt;Product&gt;())).ReturnsAsync(product);

        // Act
        var result = await _service.IncreaseStockAsync(1, 50);

        // Assert
        result.Should().BeTrue();
        product.Stock.Should().Be(150);
    }

    [Fact]
    public async Task IncreaseStockAsync_WithInvalidQuantity_ShouldThrowException()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Product", Sku = "SKU001", Price = 10M, Stock = 100 };
        _repositoryMock.Setup(r =&gt; r.GetByIdAsync(1)).ReturnsAsync(product);

        // Act &amp; Assert
        await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; _service.IncreaseStockAsync(1, 0));
    }
}
