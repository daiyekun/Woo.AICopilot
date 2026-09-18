using Woo.AICopilot.SharedKernel.Domain;

namespace Woo.AICopilot.Core.Rag.Aggregates.KnowledgeBase;

public class KnowledgeBase : IAggregateRoot
{
    private readonly List<Document> _documents = [];

    protected KnowledgeBase()
    {
    }
    
    public KnowledgeBase(string name, string description, Guid embeddingModelId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        EmbeddingModelId = embeddingModelId;
    }
    
    public Guid Id { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    
    /// <summary>
    /// 嵌入模型ID。一个知识库内的所有文档必须使用相同的嵌入模型。
    /// </summary>
    public Guid EmbeddingModelId { get; private set; }
    
    // 导航属性：对外只暴露只读集合
    public IReadOnlyCollection<Document> Documents => _documents.AsReadOnly();

    /// <summary>
    /// 添加新文档到知识库
    /// </summary>
    public Document AddDocument(string name, string filePath, string extension, string fileHash)
    {
        var document = new Document(Id, name, filePath, extension, fileHash);
        _documents.Add(document);
        return document;
    }

    /// <summary>
    /// 移除文档
    /// </summary>
    public void RemoveDocument(int documentId)
    {
        var doc = _documents.FirstOrDefault(d => d.Id == documentId);
        if (doc != null)
        {
            _documents.Remove(doc);
        }
    }

    public void UpdateInfo(string name, string description)
    {
        Name = name;
        Description = description;
    }
}