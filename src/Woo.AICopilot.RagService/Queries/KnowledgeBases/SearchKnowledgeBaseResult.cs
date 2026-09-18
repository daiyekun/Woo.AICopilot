namespace Woo.AICopilot.RagService.Queries.KnowledgeBases;

public record SearchKnowledgeBaseResult
{
    /// <summary>
    /// 检索到的文本片段
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// 相似度分数 (0.0 - 1.0)
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// 来源文档 ID (用于引用溯源)
    /// </summary>
    public int DocumentId { get; init; }
    
    /// <summary>
    /// 来源文档名称
    /// </summary>
    public string? DocumentName { get; init; }
}