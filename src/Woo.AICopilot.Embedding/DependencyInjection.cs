using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Woo.AICopilot.Embedding;

public static class DependencyInjection
{
    public static void AddEmbedding(this IHostApplicationBuilder builder)
    {
        // 1. 注册 Qdrant 客户端 (从 Aspire 注入)
        builder.AddQdrantClient("qdrant");
        
        // 2. 注册 Semantic Kernel 的 Qdrant 向量存储实现
        builder.Services.AddQdrantVectorStore();
        
        // 3. 注册嵌入生成器工厂
        builder.Services.AddSingleton<EmbeddingGeneratorFactory>();
        
        // 4. 配置嵌入服务专用的 HTTP 客户端
        builder.Services.AddHttpClient("EmbeddingClient", client =>
        {
            client.Timeout = TimeSpan.FromMinutes(20);
        });
    }
}