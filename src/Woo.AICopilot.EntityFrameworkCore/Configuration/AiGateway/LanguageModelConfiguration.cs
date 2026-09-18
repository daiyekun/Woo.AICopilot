using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Woo.AICopilot.Core.AiGateway.Aggregates.LanguageModel;

namespace Woo.AICopilot.EntityFrameworkCore.Configuration.AiGateway;

public class LanguageModelConfiguration : IEntityTypeConfiguration<LanguageModel>
{
    public void Configure(EntityTypeBuilder<LanguageModel> builder)
    {
        // 配置表名
        builder.ToTable("language_models");

        // 配置主键
        builder.HasKey(lm => lm.Id);
        builder.Property(lm => lm.Id).HasColumnName("id");

        // 配置属性
        builder.Property(lm => lm.Provider)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("provider");

        builder.Property(lm => lm.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("name");
        
        // 唯一约束
        builder.HasIndex(lm => new { lm.Provider, lm.Name })
            .IsUnique();

        builder.Property(lm => lm.BaseUrl)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("base_url");

        builder.Property(lm => lm.ApiKey)
            .HasMaxLength(100)
            .HasColumnName("api_key");

        // 配置值对象 ModelParameters
        // 这会将其属性映射为 "LanguageModels" 表中的列
        builder.OwnsOne(lm => lm.Parameters, parametersBuilder =>
        {
            parametersBuilder.Property(p => p.MaxTokens)
                .IsRequired()
                .HasColumnName("max_tokens");

            parametersBuilder.Property(p => p.Temperature)
                .IsRequired()
                .HasColumnName("temperature");
        });
    }
}