using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Woo.AICopilot.Core.Rag.Aggregates.KnowledgeBase;

namespace Woo.AICopilot.EntityFrameworkCore.Configuration.Rag;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(d => d.KnowledgeBaseId)
            .IsRequired()
            .HasColumnName("knowledge_base_id");

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(256)
            .HasColumnName("name");

        builder.Property(d => d.FilePath)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("file_path");

        builder.Property(d => d.Extension)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("extension");

        builder.Property(d => d.FileHash)
            .IsRequired()
            .HasMaxLength(64) // 使用 SHA256，通常为 64 字符
            .HasColumnName("file_hash");

        // 状态枚举：建议存为字符串，方便数据库直观查看
        builder.Property(d => d.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>() 
            .HasColumnName("status");

        builder.Property(d => d.ChunkCount)
            .IsRequired()
            .HasColumnName("chunk_count");

        builder.Property(d => d.ErrorMessage)
            .HasColumnName("error_message"); // 允许为空

        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(d => d.ProcessedAt)
            .HasColumnName("processed_at"); // 允许为空
        
        // 配置导航属性 Chunks
        builder.HasMany(d => d.Chunks)
            .WithOne(c => c.Document)
            .HasForeignKey(c => c.DocumentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); // 删除文档时级联删除切片
    }
}