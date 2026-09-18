using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Woo.AICopilot.Core.AiGateway.Aggregates.ConversationTemplate;

namespace Woo.AICopilot.EntityFrameworkCore.Configuration.AiGateway;

public class ConversationTemplateConfiguration : IEntityTypeConfiguration<ConversationTemplate>
{
    public void Configure(EntityTypeBuilder<ConversationTemplate> builder)
    {
        // 配置表名
        builder.ToTable("conversation_templates");

        // 配置主键
        builder.HasKey(ct => ct.Id);
        builder.Property(ct => ct.Id).HasColumnName("id");
        
        // 配置属性
        builder.Property(ct => ct.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("name");
        
        // 唯一约束
        builder.HasIndex(ct => ct.Name)
            .IsUnique();

        builder.Property(ct => ct.Description)
            .HasMaxLength(1000)
            .HasColumnName("description"); // 允许为空

        builder.Property(ct => ct.SystemPrompt)
            .IsRequired()
            .HasColumnName("system_prompt");

        builder.Property(ct => ct.ModelId)
            .IsRequired()
            .HasColumnName("model_id");

        builder.Property(ct => ct.IsEnabled)
            .IsRequired()
            .HasColumnName("is_enabled");

        // 配置值对象 TemplateSpecification (Record)
        builder.OwnsOne(ct => ct.Specification, specBuilder =>
        {
            // 列名将默认为 Specification_MaxTokens 等
            specBuilder.Property(s => s.MaxTokens)
                .HasColumnName("max_tokens");

            specBuilder.Property(s => s.Temperature)
                .HasColumnName("temperature");
        });
    }
}