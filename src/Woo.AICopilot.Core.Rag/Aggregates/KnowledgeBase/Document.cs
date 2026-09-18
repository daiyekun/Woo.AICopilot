using Woo.AICopilot.SharedKernel.Domain;

namespace Woo.AICopilot.Core.Rag.Aggregates.KnowledgeBase;

public class Document : IEntity<int>
{
    private readonly List<DocumentChunk> _chunks = [];

    protected Document()
    {
    }

    internal Document(Guid knowledgeBaseId, string name, string filePath, string extension, string fileHash)
    {
        KnowledgeBaseId = knowledgeBaseId;
        Name = name;
        FilePath = filePath;
        Extension = extension;
        FileHash = fileHash;
        Status = DocumentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public int Id { get; set; }
    
    public Guid KnowledgeBaseId { get; private set; }
    
    /// <summary>
    /// 原始文件名
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>
    /// 文件存储路径
    /// </summary>
    public string FilePath { get; private set; } = string.Empty;
    
    /// <summary>
    /// 文件扩展名
    /// </summary>
    public string Extension { get; private set; } = string.Empty;
    
    /// <summary>
    /// 文件哈希值
    /// </summary>
    public string FileHash { get; private set; } = string.Empty;
    
    /// <summary>
    /// 文档处理状态
    /// </summary>
    public DocumentStatus Status { get; private set; }
    
    /// <summary>
    /// 切片数量
    /// </summary>
    public int ChunkCount { get; private set; }
    
    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    // 导航属性
    public KnowledgeBase KnowledgeBase { get; private set; } = null!;
    public IReadOnlyCollection<DocumentChunk> Chunks => _chunks.AsReadOnly();

    #region 领域行为方法

    /// <summary>
    /// 开始解析文档
    /// </summary>
    public void StartParsing()
    {
        if (Status != DocumentStatus.Pending && Status != DocumentStatus.Failed)
            throw new InvalidOperationException($"当前状态 {Status} 不允许开始解析");
            
        Status = DocumentStatus.Parsing;
        ErrorMessage = null;
    }

    /// <summary>
    /// 完成解析，准备切片
    /// </summary>
    public void CompleteParsing()
    {
        if (Status != DocumentStatus.Parsing) return;
        Status = DocumentStatus.Splitting;
    }

    /// <summary>
    /// 添加文档切片
    /// </summary>
    public void AddChunk(int index, string content)
    {
        // 允许在 Splitting 或 Embedding 阶段添加/重新生成切片
        if (Status != DocumentStatus.Splitting && Status != DocumentStatus.Embedding)
             throw new InvalidOperationException($"当前状态 {Status} 不允许添加切片");

        var chunk = new DocumentChunk(Id, index, content);
        _chunks.Add(chunk);
        ChunkCount = _chunks.Count;
    }
    
    /// <summary>
    /// 清空所有切片（例如重新处理时）
    /// </summary>
    public void ClearChunks()
    {
        _chunks.Clear();
        ChunkCount = 0;
    }

    /// <summary>
    /// 开始向量化
    /// </summary>
    public void StartEmbedding()
    {
        Status = DocumentStatus.Embedding;
    }

    /// <summary>
    /// 标记切片已向量化完成（更新向量ID）
    /// </summary>
    public void MarkChunkAsEmbedded(int chunkId, string vectorId)
    {
        var chunk = _chunks.FirstOrDefault(c => c.Id == chunkId);
        chunk?.SetVectorId(vectorId);
    }

    /// <summary>
    /// 文档处理全部完成
    /// </summary>
    public void MarkAsIndexed()
    {
        Status = DocumentStatus.Indexed;
        ProcessedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 标记处理失败
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        Status = DocumentStatus.Failed;
        ErrorMessage = errorMessage;
    }

    #endregion
}

public enum DocumentStatus
{
    Pending = 0,      // 等待处理
    Parsing = 1,      // 正在读取/解析内容
    Splitting = 2,    // 正在进行文本切片
    Embedding = 3,    // 正在调用模型生成向量
    Indexed = 4,      // 索引完成，可用于检索
    Failed = 5        // 处理失败
}