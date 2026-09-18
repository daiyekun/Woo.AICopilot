using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Woo.AICopilot.Core.Rag.Aggregates.EmbeddingModel;
using Woo.AICopilot.Core.Rag.Aggregates.KnowledgeBase;
using Woo.AICopilot.Embedding;
using Woo.AICopilot.Embedding.Models;
using Woo.AICopilot.RagWorker.Models;
using Woo.AICopilot.Services.Common.Attributes;
using Woo.AICopilot.SharedKernel.Messaging;
using Woo.AICopilot.SharedKernel.Repository;
using Woo.AICopilot.SharedKernel.Result;

namespace Woo.AICopilot.RagService.Queries.KnowledgeBases;

[AuthorizeRequirement("Rag.SearchKnowledgeBase")]
public record SearchKnowledgeBaseQuery(
    Guid KnowledgeBaseId, 
    string QueryText, 
    int TopK = 3, 
    double MinScore = 0.5) 
    : IQuery<Result<List<SearchKnowledgeBaseResult>>>;
    
public class SearchKnowledgeBaseQueryHandler(
    IReadRepository<KnowledgeBase> kbRepo,
    IReadRepository<EmbeddingModel> embeddingModelRepo,
    EmbeddingGeneratorFactory embeddingFactory,
    VectorStore vectorStore)
    : IQueryHandler<SearchKnowledgeBaseQuery, Result<List<SearchKnowledgeBaseResult>>>
{
    public async Task<Result<List<SearchKnowledgeBaseResult>>> Handle(
        SearchKnowledgeBaseQuery request, 
        CancellationToken cancellationToken)
    {
        // 1. 获取知识库配置
        // 我们需要知道这个知识库绑定了哪个嵌入模型
        var kb = await kbRepo.GetByIdAsync(request.KnowledgeBaseId, cancellationToken);
        if (kb == null) return Result.NotFound("知识库不存在");

        // 2. 获取嵌入模型配置
        // 必须使用与索引时完全相同的模型配置，否则向量空间不匹配
        var embeddingModelConfig = await embeddingModelRepo.GetByIdAsync(kb.EmbeddingModelId, cancellationToken);
        if (embeddingModelConfig == null) return Result.Failure("未找到关联的嵌入模型配置");

        // 3. 生成查询向量 (Query Embedding)
        // 这里复用了 Infrastructure 中的工厂，保证了处理逻辑的一致性
        using var generator = embeddingFactory.CreateGenerator(embeddingModelConfig);
        var queryEmbedding = await generator.GenerateVectorAsync(request.QueryText, cancellationToken: cancellationToken);

        // 4. 获取向量集合 (Collection)
        // 集合名称规则必须与 RagWorker 中保存时的一致：kb-{KnowledgeBaseId}
        var collectionName = $"kb-{kb.Id:N}";
        var vectorSearchCollection = vectorStore.GetCollection<ulong, VectorDocumentRecord>(
            collectionName, 
            VectorDocumentDefinition.Get(embeddingModelConfig.Dimensions));

        // 5. 执行向量搜索
        // VectorizedSearchAsync 是 Semantic Kernel 提供的统一抽象接口
        var searchResults = vectorSearchCollection.SearchAsync(
            queryEmbedding, request.TopK, cancellationToken: cancellationToken);

        // 6. 结果映射与过滤
        var results = new List<SearchKnowledgeBaseResult>();

        await foreach (var record in searchResults)
        {
            // 应用 Score 阈值过滤，确保结果质量
            if (record.Score < request.MinScore) continue;

            results.Add(new SearchKnowledgeBaseResult
            {
                Text = record.Record.Text,
                Score = record.Score ?? 0,
                DocumentId = int.Parse(record.Record.DocumentId),
                DocumentName = record.Record.DocumentName
            });
        }

        return Result.Success(results);
    }
}