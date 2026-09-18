using Woo.AICopilot.Core.Rag.Aggregates.EmbeddingModel;
using Woo.AICopilot.Core.Rag.Aggregates.KnowledgeBase;
using Woo.AICopilot.Services.Common.Attributes;
using Woo.AICopilot.SharedKernel.Messaging;
using Woo.AICopilot.SharedKernel.Repository;
using Woo.AICopilot.SharedKernel.Result;

namespace Woo.AICopilot.RagService.Commands.KnowledgeBases;

public record CreatedKnowledgeBaseDto(Guid Id, string Name);

[AuthorizeRequirement("Rag.CreateKnowledgeBase")]
public record CreateKnowledgeBaseCommand(
    string Name, 
    string Description, 
    Guid EmbeddingModelId) : ICommand<Result<CreatedKnowledgeBaseDto>>;

public class CreateKnowledgeBaseCommandHandler(
    IRepository<KnowledgeBase> kbRepo,
    IReadRepository<EmbeddingModel> modelRepo)
    : ICommandHandler<CreateKnowledgeBaseCommand, Result<CreatedKnowledgeBaseDto>>
{
    public async Task<Result<CreatedKnowledgeBaseDto>> Handle(
        CreateKnowledgeBaseCommand request, 
        CancellationToken cancellationToken)
    {
        // 1. 校验嵌入模型是否存在
        // 知识库必须绑定一个具体的 Embedding 模型，因为这决定了向量的维度
        var embeddingModel = await modelRepo.GetByIdAsync(request.EmbeddingModelId, cancellationToken);
        if (embeddingModel == null)
        {
            return Result.NotFound("指定的嵌入模型不存在");
        }

        // 2. 创建实体
        var kb = new KnowledgeBase(request.Name, request.Description, request.EmbeddingModelId);

        // 3. 持久化
        kbRepo.Add(kb);
        await kbRepo.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreatedKnowledgeBaseDto(kb.Id, kb.Name));
    }
}