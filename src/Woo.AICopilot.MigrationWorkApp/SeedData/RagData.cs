using Woo.AICopilot.Core.Rag.Aggregates.EmbeddingModel;
using Woo.AICopilot.Core.Rag.Aggregates.KnowledgeBase;

namespace Woo.AICopilot.MigrationWorkApp.SeedData;

public static class RagData
{
    private static readonly Guid[] Guids =
    [
        Guid.NewGuid()
    ];

    public static IEnumerable<EmbeddingModel> EmbeddingModels()
    {
        var item1 = new EmbeddingModel(
            "Qwen3-4b-Q8_0", 
            "Qwen", 
            "http://127.0.0.1:1234/v1",
            "text-embedding-qwen3-embedding-4b", 
            2560, 
            32 * 1000)
        {
            Id = Guids[0]
        };

        return [item1];
    }

    public static IEnumerable<KnowledgeBase> KnowledgeBases()
    {
        var item1 = new KnowledgeBase("General", "包含公司通用的规章制度、行政流程和企业文化信息。", Guids[0]);
        var item2 = new KnowledgeBase("TechDocs", "包含技术开发规范、API文档和架构设计说明。", Guids[0]);
        return [item1, item2];
    }
}