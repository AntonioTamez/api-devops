using Microsoft.EntityFrameworkCore;
using DevOpsApi.Models;

namespace DevOpsApi.Data;

/// <summary>
/// Contexto de base de datos principal de la aplicación
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// DbSet de productos
    /// </summary>
    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar todas las configuraciones de Fluent API del assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Configuraciones adicionales si son necesarias
        // Ya está configurado en Program.cs, pero se puede extender aquí
    }
}
