using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Woo.AICopilot.EntityFrameworkCore.Configuration.DataAnalysis;

public class BusinessDatabaseConfiguration : IEntityTypeConfiguration<BusinessDatabase>
{
    public void Configure(EntityTypeBuilder<BusinessDatabase> builder)
    {
        builder.ToTable("business_databases");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("id");

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("name");
        
        // 保证名称唯一，便于路由查找
        builder.HasIndex(b => b.Name).IsUnique();

        builder.Property(b => b.Description)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("description");

        builder.Property(b => b.ConnectionString)
            .IsRequired()
            .HasMaxLength(1000)
            .HasColumnName("connection_string");

        builder.Property(b => b.Provider)
            .IsRequired()
            .HasConversion<string>() // 存储枚举字符串，增强可读性
            .HasMaxLength(50)
            .HasColumnName("provider");

        builder.Property(b => b.IsEnabled)
            .IsRequired()
            .HasColumnName("is_enabled");

        builder.Property(b => b.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");
    }
}