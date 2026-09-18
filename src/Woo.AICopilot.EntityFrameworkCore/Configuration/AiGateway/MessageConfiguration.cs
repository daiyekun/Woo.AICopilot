using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Woo.AICopilot.Core.AiGateway.Aggregates.Sessions;

namespace Woo.AICopilot.EntityFrameworkCore.Configuration.AiGateway;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        // 配置表名
        builder.ToTable("messages");

        // 配置主键 (int 类型，自动增长)
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // 配置属性
        builder.Property(m => m.Content)
            .IsRequired()
            .HasColumnName("content");

        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at"); //

        // 配置枚举 MessageType
        // 将枚举存储为字符串（"User", "Assistant"）而不是整数
        builder.Property(m => m.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("type"); // <--- 修改

        // 外键属性的列名改为小写
        builder.Property(m => m.SessionId)
            .IsRequired()
            .HasColumnName("session_id"); // <--- 修改

        // 配置与 Session 的多对一关系
        builder.HasOne(m => m.Session) // Message 有一个 Session
            .WithMany(s => s.Messages) // Session 有多个 Messages
            .HasForeignKey(m => m.SessionId) // 外键是 SessionId
            .HasConstraintName("fk_messages_sessions_session_id")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); // 当 Session 被删除时，其 Messages 也被删除
    }
}