using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Woo.AICopilot.Core.Rag.Aggregates.KnowledgeBase;
using Woo.AICopilot.Embedding;
using Woo.AICopilot.Embedding.Models;
using Woo.AICopilot.EntityFrameworkCore;
using Woo.AICopilot.RagWorker.Models;
using Woo.AICopilot.RagWorker.Services.Parsers;
using Woo.AICopilot.Services.Common.Contracts;

namespace Woo.AICopilot.RagWorker.Services;

public class RagService(
    IFileStorageService fileStorage,
    DocumentParserFactory parserFactory,
    TextSplitterService textSplitter,
    EmbeddingGeneratorFactory embeddingFactory,
    VectorStore vectorStoreClient,
    AiCopilotDbContext dbContext,
    ILogger<RagService> logger)
{
    public async Task IndexDocumentAsync(Document document, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("开始索引流程: {DocumentName}", document.Name);

        // Step 1: 加载
        var stream = await LoadDocumentAsync(document, cancellationToken);

        // Step 2: 解析
        var text = await ParseDocumentAsync(document, stream, cancellationToken);

        // Step 3: 切片
        var paragraphs = await SplitDocumentAsync(document, text, cancellationToken);

        // Step 4: 嵌入
        var (embeddings, dimensions) = await GenerateEmbeddingsAsync(document, paragraphs, cancellationToken);

        // Step 5: 保存向量并完成索引
        await SaveVectorAsync(document, paragraphs, embeddings, dimensions, cancellationToken);

        logger.LogInformation("文档索引完成: {DocumentName}", document.Name);
    }

    // ================================================================
    // Step 1: 加载
    // ================================================================
    private async Task<Stream> LoadDocumentAsync(Document document, CancellationToken ct)
    {
        logger.LogInformation("加载文档...");

        // 从存储中获取文件流
        var stream = await fileStorage.GetAsync(document.FilePath, ct);

        return stream ?? throw new FileNotFoundException($"文件未找到: {document.FilePath}");
    }

    // ================================================================
    // Step 2: 解析
    // ================================================================
    private async Task<string> ParseDocumentAsync(Document document, Stream stream, CancellationToken ct)
    {
        logger.LogInformation("解析文档...");

        // 根据扩展名获取解析器
        var parser = parserFactory.GetParser(document.Extension);

        // 提取文本
        var text = await parser.ParseAsync(stream, ct);

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("文档内容为空或无法提取文本。");

        logger.LogInformation("文本提取完成，长度: {Length} 字符", text.Length);

        // 更新状态：解析完成 -> 准备切片
        document.CompleteParsing();
        await dbContext.SaveChangesAsync(ct);

        return text;
    }

    // ================================================================
    // Step 3: 切片
    // ================================================================
    private async Task<List<string>> SplitDocumentAsync(Document document, string text, CancellationToken ct)
    {
        logger.LogInformation("开始文本切片...");

        // 为了支持重新索引，如果文档之前处理过，需要先清理旧的切片
        if (document.Chunks.Count > 0)
            document.ClearChunks();

        var paragraphs = textSplitter.Split(text);

        logger.LogInformation("文本切片完成，共 {Count} 个切片。", paragraphs.Count);

        // 将切片转换为领域实体
        for (var i = 0; i < paragraphs.Count; i++)
            document.AddChunk(i, paragraphs[i]);

        await dbContext.SaveChangesAsync(ct);

        return paragraphs;
    }

    // ================================================================
    // Step 4: 嵌入
    // ================================================================
    private async Task<(List<Embedding<float>>, int)> GenerateEmbeddingsAsync(
        Document document,
        List<string> paragraphs,
        CancellationToken ct)
    {
        logger.LogInformation("开始生成嵌入向量...");

        // 获取嵌入模型配置
        var embeddingModelConfig = await dbContext.EmbeddingModels.AsNoTracking()
            .FirstOrDefaultAsync(em => em.Id == document.KnowledgeBase.EmbeddingModelId,
                cancellationToken: ct);
        
        if (embeddingModelConfig == null)
        {
            throw new InvalidOperationException($"未找到 ID 为 {document.KnowledgeBase.EmbeddingModelId} 的嵌入模型配置");
        }
        
        // 创建嵌入生成器
        using var generator = embeddingFactory.CreateGenerator(embeddingModelConfig);

        // 准备分批
        // [配置建议]
        // - 本地模型: 建议 20 ~ 50 (取决于显卡)
        // - 云端模型: 建议 50 ~ 100
        const int batchSize = 50;
        
        // 用于收集所有生成的向量结果
        var allEmbeddings = new List<Embedding<float>>();
        
        // 将段落切分为多个批次
        var batches = paragraphs.Chunk(batchSize).ToArray();

        logger.LogInformation("共 {Paragraphs} 个段落，将分为 {Batches} 批处理", paragraphs.Count, batches.Length);

        // 循环处理每一批
        for (var i = 0; i < batches.Length; i++)
        {
            logger.LogInformation("正在处理第 {Current}/{Total} 批...", i + 1, batches.Length);

            try
            {
                var batch = batches[i];
                // 调用模型生成当前批次的向量
                var result = await generator.GenerateAsync(batch, cancellationToken: ct);
                // 将结果添加到总列表中
                allEmbeddings.AddRange(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "第 {Batch} 批次向量化失败", i + 1);
                throw;
            }
        }

        var dimensions = allEmbeddings.First().Vector.Length;
        logger.LogInformation("向量化完成，共生成 {Count} 个向量，维度: {Dim}", allEmbeddings.Count, dimensions);

        return (allEmbeddings, dimensions);
    }

    // ================================================================
    // Step 5: 保存向量
    // ================================================================
    private async Task SaveVectorAsync(
        Document document,
        List<string> chunks,
        List<Embedding<float>> embeddings,
        int dimensions,
        CancellationToken ct)
    {
        logger.LogInformation("保存向量数据...");

        // 基础参数校验
        if (chunks.Count != embeddings.Count)
        {
            throw new ArgumentException($"切片数量 ({chunks.Count}) 与向量数量 ({embeddings.Count}) 不一致");
        }

        if (chunks.Count == 0)
        {
            logger.LogWarning("文档 {DocumentId} 没有切片需要存储", document.Id);
        }

        // 2. 确定集合名称
        // 使用 "kb-" 前缀加上知识库 ID (Guid) 作为集合名，确保名称符合 Qdrant 规范且唯一
        var collectionName = $"kb-{document.KnowledgeBaseId:N}";
        logger.LogInformation("文档 {DocumentName} 将存入集合: {CollectionName}", document.Name, collectionName);
        
        // 3. 获取动态集合
        var definition = VectorDocumentDefinition.Get(dimensions);
        var collection = vectorStoreClient.GetCollection<ulong, VectorDocumentRecord>(collectionName, definition);

        // 4. 确保集合存在
        // 第一次向该知识库上传文档时，会自动创建集合
        await collection.EnsureCollectionExistsAsync(ct);

        // 5. 组装存储记录
        try
        {
            for (var i = 0; i < chunks.Count; i++)
            {
                // 生成一个唯一的记录键值
                var recordKey = (ulong)document.Id.GetHashCode() << 32 | (uint)i;

                // await collection.UpsertAsync(new Dictionary<string, object?>
                // {
                //     { "Key", recordKey },
                //     { "Text", chunks[i] },
                //     { "DocumentId", document.Id.ToString() },
                //     { "KnowledgeBaseId", document.KnowledgeBaseId.ToString() },
                //     { "ChunkIndex", i },
                //     { "Embedding", embeddings[i].Vector }
                // }, ct);

                await collection.UpsertAsync(new VectorDocumentRecord()
                {
                    Key = recordKey,
                    Text = chunks[i],
                    DocumentId = document.Id.ToString(),
                    DocumentName = document.Name,
                    KnowledgeBaseId = document.KnowledgeBaseId.ToString(),
                    ChunkIndex = i,
                    Embedding = embeddings[i].Vector
                }, ct);
            }

            logger.LogInformation("成功向集合 {Collection} 写入 {Count} 条向量记录。", collectionName, chunks.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "写入向量数据库失败。Collection: {Collection}", collectionName);
            throw;
        }

        document.MarkAsIndexed();
        await dbContext.SaveChangesAsync(ct);
    }
}