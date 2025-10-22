using DevOpsApi.Data;
using Microsoft.EntityFrameworkCore;

namespace DevOpsApi.Extensions;

/// <summary>
/// Extensiones para manejo de base de datos
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Aplica migraciones pendientes automáticamente con retry logic
    /// </summary>
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        try
        {
            logger.LogInformation("🔄 Checking database connection and applying migrations...");

            // Retry logic para esperar a que SQL Server esté listo
            var maxRetries = 10;
            var retryDelay = TimeSpan.FromSeconds(3);

            for (int retry = 1; retry <= maxRetries; retry++)
            {
                try
                {
                    // Verificar si puede conectar
                    var canConnect = await context.Database.CanConnectAsync();
                    
                    if (!canConnect)
                    {
                        throw new Exception("Cannot connect to database");
                    }

                    logger.LogInformation("✅ Database connection established");

                    // Obtener migraciones pendientes
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                    var pendingCount = pendingMigrations.Count();

                    if (pendingCount > 0)
                    {
                        logger.LogInformation("📝 Found {Count} pending migration(s). Applying...", pendingCount);
                        foreach (var migration in pendingMigrations)
                        {
                            logger.LogInformation("  - {Migration}", migration);
                        }

                        await context.Database.MigrateAsync();
                        logger.LogInformation("✅ Database migrations applied successfully");
                    }
                    else
                    {
                        logger.LogInformation("✅ Database is up to date. No pending migrations");
                    }

                    // Seed data
                    await SeedDataAsync(context, logger);

                    break; // Éxito - salir del loop
                }
                catch (Exception ex) when (retry < maxRetries)
                {
                    logger.LogWarning(
                        "⚠️ Could not apply migrations (attempt {Retry}/{MaxRetries}): {Message}",
                        retry, maxRetries, ex.Message);
                    
                    logger.LogInformation("⏳ Waiting {Seconds} seconds before retry...", retryDelay.TotalSeconds);
                    await Task.Delay(retryDelay);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ An error occurred while migrating the database");
            
            // En desarrollo, lanzar excepción para detener la app
            if (app.Environment.IsDevelopment())
            {
                throw;
            }
            
            // En producción, solo loguear y continuar
            logger.LogWarning("⚠️ Application will start without database migrations");
        }
    }

    /// <summary>
    /// Aplica seed data inicial a la base de datos
    /// </summary>
    private static async Task SeedDataAsync(ApplicationDbContext context, ILogger logger)
    {
        try
        {
            // Verificar si ya hay productos
            if (await context.Products.AnyAsync())
            {
                logger.LogInformation("ℹ️ Database already contains data. Skipping seed");
                return;
            }

            logger.LogInformation("🌱 Seeding initial data...");

            // Crear productos de ejemplo
            var products = new[]
            {
                new Models.Product
                {
                    Name = "Laptop Dell XPS 15",
                    Description = "High performance laptop with Intel i7 and 16GB RAM",
                    Sku = "DELL-XPS15-001",
                    Price = 1299.99m,
                    Stock = 50,
                    Category = "Electronics",
                    IsActive = true
                },
                new Models.Product
                {
                    Name = "Wireless Mouse Logitech MX Master 3",
                    Description = "Ergonomic wireless mouse for productivity",
                    Sku = "LOGI-MX3-001",
                    Price = 99.99m,
                    Stock = 150,
                    Category = "Accessories",
                    IsActive = true
                },
                new Models.Product
                {
                    Name = "Mechanical Keyboard Keychron K2",
                    Description = "Compact wireless mechanical keyboard",
                    Sku = "KEY-K2-001",
                    Price = 79.99m,
                    Stock = 100,
                    Category = "Accessories",
                    IsActive = true
                },
                new Models.Product
                {
                    Name = "USB-C Hub Anker 7-in-1",
                    Description = "Multi-port USB-C hub with HDMI and SD card reader",
                    Sku = "ANK-HUB-001",
                    Price = 49.99m,
                    Stock = 200,
                    Category = "Accessories",
                    IsActive = true
                },
                new Models.Product
                {
                    Name = "Monitor LG 27 UltraFine 4K",
                    Description = "27-inch 4K UHD IPS monitor",
                    Sku = "LG-27UK-001",
                    Price = 599.99m,
                    Stock = 30,
                    Category = "Electronics",
                    IsActive = true
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            logger.LogInformation("✅ Seed data applied successfully. Added {Count} products", products.Length);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ An error occurred while seeding data");
            // No lanzar excepción - seed es opcional
        }
    }
}
