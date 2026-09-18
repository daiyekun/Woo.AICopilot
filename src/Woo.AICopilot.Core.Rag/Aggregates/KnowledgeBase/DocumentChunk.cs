using Woo.AICopilot.SharedKernel.Domain;

namespace Woo.AICopilot.Core.Rag.Aggregates.KnowledgeBase;

public class DocumentChunk : IEntity<int>
{
    protected DocumentChunk()
    {
    }

    internal DocumentChunk(int documentId, int index, string content)
    {
        DocumentId = documentId;
        Index = index;
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }

    public int Id { get; set; }
    
    public int DocumentId { get; private set; }
    
    /// <summary>
    /// 切片序号
    /// </summary>
    public int Index { get; private set; }
    
    /// <summary>
    /// 文本内容
    /// </summary>
    public string Content { get; private set; } = string.Empty;
    
    /// <summary>
    /// 向量数据库中的ID
    /// </summary>
    public string? VectorId { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    // 导航属性
    public Document Document { get; private set; } = null!;

    /// <summary>
    /// 设置向量ID (当向量化完成后调用)
    /// </summary>
    public void SetVectorId(string vectorId)
    {
        VectorId = vectorId;
    }
}