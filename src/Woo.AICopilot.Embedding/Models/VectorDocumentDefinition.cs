using Microsoft.Extensions.VectorData;

namespace Woo.AICopilot.Embedding.Models;

public static class VectorDocumentDefinition
{
    public static VectorStoreCollectionDefinition Get(int dimensions)
    {
        VectorStoreCollectionDefinition definition = new()
        {
            Properties = new List<VectorStoreProperty>
            {
                new VectorStoreKeyProperty("Key", typeof(ulong)),
                new VectorStoreDataProperty("Text", typeof(string)) { IsFullTextIndexed = true },
                new VectorStoreDataProperty("DocumentId", typeof(string)){ IsIndexed = true },
                new VectorStoreDataProperty("DocumentName", typeof(string)),
                new VectorStoreDataProperty("KnowledgeBaseId", typeof(string)){ IsIndexed = true },
                new VectorStoreDataProperty("ChunkIndex", typeof(int)),
                new VectorStoreVectorProperty("Embedding", typeof(ReadOnlyMemory<float>),
                    dimensions: dimensions)
                {
                    DistanceFunction = DistanceFunction.CosineSimilarity,
                    IndexKind = IndexKind.Hnsw
                }
            }
        };
        return definition;
    }
}