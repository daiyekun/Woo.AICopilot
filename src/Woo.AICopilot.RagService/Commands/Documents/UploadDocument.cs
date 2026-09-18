using System.Security.Cryptography;
using MassTransit;
using Woo.AICopilot.Core.Rag.Aggregates.KnowledgeBase;
using Woo.AICopilot.Services.Common.Attributes;
using Woo.AICopilot.Services.Common.Contracts;
using Woo.AICopilot.Services.Common.Events;
using Woo.AICopilot.SharedKernel.Messaging;
using Woo.AICopilot.SharedKernel.Repository;
using Woo.AICopilot.SharedKernel.Result;

namespace Woo.AICopilot.RagService.Commands.Documents;

public record UploadDocumentDto(int Id, string Status);

public record FileUploadStream(string FileName, Stream Stream);

[AuthorizeRequirement("Rag.UploadDocument")]
public record UploadDocumentCommand(
    Guid KnowledgeBaseId, 
    FileUploadStream File) : ICommand<Result<UploadDocumentDto>>;

public class UploadDocumentCommandHandler(
    IRepository<KnowledgeBase> kbRepo,
    IFileStorageService fileStorage,
    IPublishEndpoint publishEndpoint) 
    : ICommandHandler<UploadDocumentCommand, Result<UploadDocumentDto>>
{
    public async Task<Result<UploadDocumentDto>> Handle(
        UploadDocumentCommand request, 
        CancellationToken cancellationToken)
    {
        // 1. 获取知识库聚合根（并急切加载 Documents 集合）
        // 使用我们刚扩展的 GetAsync 方法，通过 includes 参数加载子实体
        var kb = await kbRepo.GetAsync(
            kb => kb.Id == request.KnowledgeBaseId, 
            includes: [k => k.Documents], 
            cancellationToken);

        if (kb == null) return Result.NotFound("知识库不存在");
        
        // 2. 计算文件 Hash (SHA256)
        string fileHash;
        using (var sha256 = SHA256.Create())
        {
            // 确保流从头开始
            if (request.File.Stream.CanSeek) request.File.Stream.Position = 0;
            
            var hashBytes = await sha256.ComputeHashAsync(request.File.Stream, cancellationToken);
            fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            
            // 计算完 Hash 后，必须重置流位置，否则后续保存文件时会读到空内容
            if (request.File.Stream.CanSeek) request.File.Stream.Position = 0;
        }
        
        // 3. 检查文件是否已存在 (基于 Hash 实现幂等性)
        // 因为 Documents 已经加载到内存中，我们可以直接使用 LINQ 查询
        var existingDoc = kb.Documents.FirstOrDefault(d => d.FileHash == fileHash);
        if (existingDoc != null)
        {
            // 如果文件已存在，直接返回成功，并返回现有的文档 ID
            // 这实现了接口的幂等性：多次上传同一文件不会产生副作用
            return Result.Success(new UploadDocumentDto(existingDoc.Id, existingDoc.Status.ToString()));
        }
        
        // 4. 保存物理文件 (只有当文件不存在时才执行 IO 操作)
        var extension = Path.GetExtension(request.File.FileName).ToLower();
        var savedPath = await fileStorage.SaveAsync(request.File.Stream, request.File.FileName, cancellationToken);

        // 5. 领域模型行为：添加文档
        // 这一步是纯内存操作，修改了聚合根的状态
        var document = kb.AddDocument(request.File.FileName, savedPath, extension, fileHash);

        // 6. 持久化到数据库
        await kbRepo.SaveChangesAsync(cancellationToken);

        // 7. 发送集成事件 (通知后台 Worker 开始索引)
        await publishEndpoint.Publish(new DocumentUploadedEvent
        {
            DocumentId = document.Id,
            KnowledgeBaseId = kb.Id,
            FilePath = savedPath,
            FileName = request.File.FileName
        }, cancellationToken);

        return Result.Success(new UploadDocumentDto(document.Id, document.Status.ToString()));
    }
}