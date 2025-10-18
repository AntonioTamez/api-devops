using Microsoft.EntityFrameworkCore;

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

    // DbSets se agregarán aquí cuando se creen las entidades
    // public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuraciones de Fluent API se agregarán aquí
        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Configuraciones adicionales si son necesarias
        // Ya está configurado en Program.cs, pero se puede extender aquí
    }
}
