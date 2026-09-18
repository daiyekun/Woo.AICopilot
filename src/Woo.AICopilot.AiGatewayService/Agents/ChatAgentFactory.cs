using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using Woo.AICopilot.Core.AiGateway.Aggregates.ConversationTemplate;
using Woo.AICopilot.Core.AiGateway.Aggregates.LanguageModel;
using Woo.AICopilot.Services.Common.Contracts;

namespace Woo.AICopilot.AiGatewayService.Agents;

public class ChatAgentFactory(IServiceProvider serviceProvider)
{
    private async Task<(LanguageModel, ConversationTemplate)> GetModelAndTemplateAsync(
        Expression<Func<ConversationTemplate, bool>> predicate)
    {
        var data = serviceProvider.GetRequiredService<IDataQueryService>();
        var query =
            from template in data.ConversationTemplates.Where(predicate)
            join model in data.LanguageModels on template.ModelId equals model.Id
            select new { model, template };

        var result = await data.FirstOrDefaultAsync(query);
        if (result == null) throw new Exception("未找对话模板或模型");
        return (result.model, result.template);
    }
    
    public ChatClientAgent CreateAgentAsync(LanguageModel model, 
        ConversationTemplate template,
        Action<ChatOptions>? configureOptions = null,
        bool isSaveChatMessage = true)
    {
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient("OpenAI");

        var chatClientBuilder = new OpenAIClient(
                new ApiKeyCredential(model.ApiKey ?? string.Empty),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(model.BaseUrl),
                    Transport = new HttpClientPipelineTransport(httpClient)
                })
            .GetChatClient(model.Name)
            .AsIChatClient()
            .AsBuilder()
            .UseOpenTelemetry(sourceName: nameof(AiGatewayService), configure: cfg => cfg.EnableSensitiveData = true);

        var chatOptions = new ChatOptions
        {
            Instructions = template.SystemPrompt,
            Temperature = template.Specification.Temperature ?? model.Parameters.Temperature
        };
        
        // 执行外部传入的配置逻辑（例如挂载工具）
        configureOptions?.Invoke(chatOptions);

        var agentOptions = new ChatClientAgentOptions
        {
            Name = template.Name,
            ChatOptions = chatOptions
        };
        
        // 动态创建会话存储
        if (isSaveChatMessage)
        {
            agentOptions.ChatMessageStoreFactory = context =>
                new SessionChatMessageStore(serviceProvider, context.SerializedState);
        }
        
        var agent = chatClientBuilder.BuildAIAgent(agentOptions, services: serviceProvider);
        
        return agent;
    }

    public async Task<ChatClientAgent> CreateAgentAsync(
        Guid templateId, 
        Action<ChatOptions>? configureOptions = null, 
        bool isSaveChatMessage = true)
    {
        var (model, template) = await GetModelAndTemplateAsync(t => t.Id == templateId);
        return CreateAgentAsync(model, template, configureOptions, isSaveChatMessage);
    }
    
    public async Task<ChatClientAgent> CreateAgentAsync(string templateName, 
        Action<ConversationTemplate>? configureTemplate = null,
        Action<ChatOptions>? configureOptions = null,
        bool isSaveChatMessage = true)
    {
        var (model, template) = await GetModelAndTemplateAsync(t => t.Name == templateName);
        configureTemplate?.Invoke(template);
        return CreateAgentAsync(model, template, configureOptions, isSaveChatMessage);
    }
}