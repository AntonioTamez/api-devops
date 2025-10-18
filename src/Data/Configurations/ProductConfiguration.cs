using DevOpsApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsApi.Data.Configurations;

/// <summary>
/// Configuración de Entity Framework para la entidad Product usando Fluent API
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Configuración de tabla
        builder.ToTable("Products");

        // Configuración de clave primaria
        builder.HasKey(p => p.Id);

        // Configuración de propiedades
        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.Stock)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.Category)
            .HasMaxLength(50);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(p => p.UpdatedAt)
            .IsRequired(false);

        // Índices
        // Índice único para SKU (no puede haber productos con el mismo SKU)
        builder.HasIndex(p => p.Sku)
            .IsUnique()
            .HasDatabaseName("IX_Products_Sku");

        // Índice para búsquedas por nombre
        builder.HasIndex(p => p.Name)
            .HasDatabaseName("IX_Products_Name");

        // Índice para búsquedas por categoría
        builder.HasIndex(p => p.Category)
            .HasDatabaseName("IX_Products_Category");

        // Índice compuesto para filtrar productos activos por categoría
        builder.HasIndex(p => new { p.IsActive, p.Category })
            .HasDatabaseName("IX_Products_IsActive_Category");

        // Restricciones de validación (Check Constraints)
        builder.ToTable(t => t.HasCheckConstraint("CK_Products_Price", "[Price] > 0"));
        builder.ToTable(t => t.HasCheckConstraint("CK_Products_Stock", "[Stock] >= 0"));
    }
}
