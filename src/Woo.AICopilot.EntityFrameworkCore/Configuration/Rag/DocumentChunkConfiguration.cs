using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Woo.AICopilot.Core.Rag.Aggregates.KnowledgeBase;

namespace Woo.AICopilot.EntityFrameworkCore.Configuration.Rag;

public class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.ToTable("document_chunks");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.DocumentId)
            .IsRequired()
            .HasColumnName("document_id");

        builder.Property(c => c.Index)
            .IsRequired()
            .HasColumnName("index");

        // 内容字段，根据数据库类型可能需要配置为 TEXT
        builder.Property(c => c.Content)
            .IsRequired()
            .HasColumnType("text")
            .HasColumnName("content");

        builder.Property(c => c.VectorId)
            .HasMaxLength(100)
            .HasColumnName("vector_id"); // 允许为空，因为刚切分完可能还没向量化

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");
            
        // 索引配置：通常会根据文档ID查询切片，并按顺序排序
        builder.HasIndex(c => new { c.DocumentId, c.Index })
            .IsUnique(); // 保证同一文档内切片序号不重复
    }
}