using DevOpsApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DevOpsApi.UnitTests.Helpers;

/// <summary>
/// Clase base para tests unitarios con helpers comunes
/// </summary>
public abstract class TestBase : IDisposable
{
    protected ApplicationDbContext Context { get; private set; }
    private bool _disposed = false;

    protected TestBase()
    {
        Context = CreateInMemoryDbContext();
    }

    /// <summary>
    /// Crea un DbContext en memoria para tests
    /// </summary>
    protected ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Crea un mock de ILogger para cualquier tipo
    /// </summary>
    protected Mock<ILogger<T>> CreateLoggerMock<T>()
    {
        return new Mock<ILogger<T>>();
    }

    /// <summary>
    /// Limpia los recursos del test
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Context?.Database.EnsureDeleted();
                Context?.Dispose();
            }
            _disposed = true;
        }
    }
}
