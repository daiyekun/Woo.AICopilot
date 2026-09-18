using Microsoft.Extensions.VectorData;

namespace Woo.AICopilot.RagWorker.Models;

/// <summary>
/// 对应向量数据库中的一条记录
/// </summary>
public class VectorDocumentRecord
{
    /// <summary>
    /// 记录的唯一标识符
    /// </summary>
    /// <remarks>
    /// 使用 ulong 类型，因为 Qdrant 内部 ID 支持 64 位无符号整数或 UUID。
    /// 这里我们不使用 Guid，而是为了与语义对齐，将在存储时生成唯一 ID。
    /// </remarks>
    [VectorStoreKey]
    public ulong Key { get; set; }

    /// <summary>
    /// 原始文本内容
    /// </summary>
    [VectorStoreData(IsFullTextIndexed = true)]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// 关联的文档 ID (元数据)
    /// </summary>
    /// <remarks>
    /// IsFilterable = true 允许我们在检索时按 DocumentId 过滤，
    /// 例如：只查询特定文档的内容。
    /// </remarks>
    [VectorStoreData(IsIndexed = true)]
    public string DocumentId { get; set; } = string.Empty;
    
    /// <summary>
    /// 关联的文档名称 (元数据)
    /// </summary>
    public string DocumentName { get; set; } = string.Empty;

    /// <summary>
    /// 关联的知识库 ID (元数据)
    /// </summary>
    [VectorStoreData(IsIndexed = true)]
    public string KnowledgeBaseId { get; set; } = string.Empty;

    /// <summary>
    /// 原始切片在文档中的索引顺序
    /// </summary>
    [VectorStoreData]
    public int ChunkIndex { get; set; }

    /// <summary>
    /// 嵌入向量
    /// </summary>
    /// <remarks>
    /// Dimensions 必须与我们使用的模型（Qwen3-4B）一致，否则插入会报错。
    /// DistanceFunction 定义了相似度计算方式，Cosine (余弦相似度) 是文本检索的标准选择。
    /// </remarks>
    [VectorStoreVector(Dimensions: 2560, DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw)]
    public ReadOnlyMemory<float> Embedding { get; set; }
}