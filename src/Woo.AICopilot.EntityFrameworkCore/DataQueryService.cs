using Microsoft.EntityFrameworkCore;
using Woo.AICopilot.Core.AiGateway.Aggregates.ConversationTemplate;
using Woo.AICopilot.Core.AiGateway.Aggregates.LanguageModel;
using Woo.AICopilot.Core.AiGateway.Aggregates.Sessions;
using Woo.AICopilot.Core.McpServer.Aggregates.McpServerInfo;
using Woo.AICopilot.Core.Rag.Aggregates.EmbeddingModel;
using Woo.AICopilot.Core.Rag.Aggregates.KnowledgeBase;
using Woo.AICopilot.Services.Common.Contracts;

namespace Woo.AICopilot.EntityFrameworkCore;

public class DataQueryService(AiCopilotDbContext dbContext) : IDataQueryService
{
    public IQueryable<ConversationTemplate> ConversationTemplates => dbContext.ConversationTemplates.AsNoTracking();
    public IQueryable<LanguageModel> LanguageModels => dbContext.LanguageModels.AsNoTracking();
    public IQueryable<Session> Sessions => dbContext.Sessions.AsNoTracking();
    public IQueryable<Message> Messages => dbContext.Messages.AsNoTracking();
    
    public IQueryable<EmbeddingModel> EmbeddingModels => dbContext.EmbeddingModels.AsNoTracking();
    public IQueryable<KnowledgeBase> KnowledgeBases => dbContext.KnowledgeBases.AsNoTracking();
    public IQueryable<Document> Documents => dbContext.Documents.AsNoTracking();
    public IQueryable<DocumentChunk> DocumentChunks => dbContext.DocumentChunks.AsNoTracking();

    public IQueryable<BusinessDatabase> BusinessDatabases => dbContext.BusinessDatabases.AsNoTracking();
    public IQueryable<McpServerInfo> McpServerInfos => dbContext.McpServerInfos.AsNoTracking();
    

    public async Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> queryable) where T : class
    {
        return await queryable.AsNoTracking().FirstOrDefaultAsync();
    }

    public async Task<IList<T>> ToListAsync<T>(IQueryable<T> queryable) where T : class
    {
        return await queryable.AsNoTracking().ToListAsync();
    }

    public async Task<bool> AnyAsync<T>(IQueryable<T> queryable) where T : class
    {
        return await queryable.AsNoTracking().AnyAsync();
    }
}