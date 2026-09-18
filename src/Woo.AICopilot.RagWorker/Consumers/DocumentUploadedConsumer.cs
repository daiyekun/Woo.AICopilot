using MassTransit;
using Microsoft.EntityFrameworkCore;
using Woo.AICopilot.Core.Rag.Aggregates.KnowledgeBase;
using Woo.AICopilot.EntityFrameworkCore;
using Woo.AICopilot.RagWorker.Services;
using Woo.AICopilot.Services.Common.Events;

namespace Woo.AICopilot.RagWorker.Consumers;

public class DocumentUploadedConsumer(
    AiCopilotDbContext dbContext,
    RagService ragService,
    ILogger<DocumentUploadedConsumer> logger) 
    : IConsumer<DocumentUploadedEvent>
{
    public async Task Consume(ConsumeContext<DocumentUploadedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("接收到文档处理请求: {DocumentId}, 文件: {FileName}", message.DocumentId, message.FileName);

        // 1. 获取文档实体 (包含 KnowledgeBase 信息)
        var document = await dbContext.Documents
            .Include(d => d.KnowledgeBase)
            .FirstOrDefaultAsync(d => d.Id == message.DocumentId);

        if (document == null)
        {
            logger.LogWarning("文档 {DocumentId} 未在数据库中找到，跳过处理。", message.DocumentId);
            return;
        }

        // 2. 幂等性与状态检查
        // 如果文档已经处理成功(Indexed)或正在处理中(Parsing/Splitting/Embedding)，则忽略
        // 除非是 Failed 状态，才允许重试
        if (document.Status != DocumentStatus.Pending && document.Status != DocumentStatus.Failed)
        {
            logger.LogInformation("文档 {DocumentId} 当前状态为 {Status}，无需重复处理。", message.DocumentId, document.Status);
            return;
        }

        try
        {
            // 3. 开始处理 - 状态流转
            document.StartParsing();
            await dbContext.SaveChangesAsync();

            // TODO: 调用核心 ETL 流程 (Parse -> Split -> Embed -> Store)
            await ragService.IndexDocumentAsync(document); 
            
            // 模拟处理成功
            logger.LogInformation("文档 {DocumentId} 索引流程执行完毕。", message.DocumentId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "文档 {DocumentId} 处理失败。", message.DocumentId);
            
            // 4. 异常处理 - 记录错误状态
            // 重新从数据库获取最新状态（防止并发冲突），标记为失败
            var errorDoc = await dbContext.Documents.FindAsync(message.DocumentId);
            if (errorDoc != null)
            {
                errorDoc.MarkAsFailed(ex.Message);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}