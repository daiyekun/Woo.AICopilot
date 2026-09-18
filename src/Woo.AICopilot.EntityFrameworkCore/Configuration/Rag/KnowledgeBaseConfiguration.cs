using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Woo.AICopilot.Core.Rag.Aggregates.KnowledgeBase;

namespace Woo.AICopilot.EntityFrameworkCore.Configuration.Rag;

public class KnowledgeBaseConfiguration : IEntityTypeConfiguration<KnowledgeBase>
{
    public void Configure(EntityTypeBuilder<KnowledgeBase> builder)
    {
        builder.ToTable("knowledge_bases");

        builder.HasKey(kb => kb.Id);
        builder.Property(kb => kb.Id).HasColumnName("id");

        builder.Property(kb => kb.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("name");

        builder.Property(kb => kb.Description)
            .HasMaxLength(1000)
            .HasColumnName("description");

        builder.Property(kb => kb.EmbeddingModelId)
            .IsRequired()
            .HasColumnName("embedding_model_id");
        
        // 配置导航属性 Documents
        builder.HasMany(kb => kb.Documents)
            .WithOne(d => d.KnowledgeBase)
            .HasForeignKey(d => d.KnowledgeBaseId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); // 删除知识库时级联删除文档
    }
}