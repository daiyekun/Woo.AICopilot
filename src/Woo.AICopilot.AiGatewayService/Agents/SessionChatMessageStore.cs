using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Woo.AICopilot.AiGatewayService.Queries.Sessions;
using Woo.AICopilot.Core.AiGateway.Aggregates.Sessions;
using Woo.AICopilot.SharedKernel.Repository;

namespace Woo.AICopilot.AiGatewayService.Agents;

public record SessionSoreState(Guid SessionId, int MessageCount = 20);

public class SessionChatMessageStore : ChatMessageStore
{
    private readonly SessionSoreState? _sessionSoreState;
    
    private readonly IServiceProvider _serviceProvider;

    public SessionChatMessageStore(IServiceProvider serviceProvider, JsonElement storeState)
    {
        _serviceProvider = serviceProvider;
        if (storeState.ValueKind is JsonValueKind.Object)
        {
            _sessionSoreState = storeState.Deserialize<SessionSoreState>()!;
        }
    }

    public override async Task<IEnumerable<ChatMessage>> GetMessagesAsync(CancellationToken cancellationToken = new())
    {
        if (_sessionSoreState == null) return [];
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var query = new GetListChatMessagesQuery(_sessionSoreState.SessionId, _sessionSoreState.MessageCount);
        var result = await mediator.Send(query, cancellationToken);
        return result.Value!;
    }

    public override async Task AddMessagesAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = new())
    {
        if (_sessionSoreState == null) return;
        
        var repo = _serviceProvider.GetRequiredService<IRepository<Session>>();

        // 加载聚合根
        var session = await repo.GetByIdAsync(_sessionSoreState.SessionId, cancellationToken);
        if (session == null) return;

        var hasNewMessage = false;
        foreach (var msg in messages)
        {
            // 将 Agent Role 转换为枚举
            // msg.Role.ToString() 可能返回 "user", "assistant" 等
            var roleStr = msg.Role.ToString().ToLower(); 
            var msgType = roleStr switch
            {
                "user" => MessageType.User,
                "assistant" => MessageType.Assistant,
                "system" => MessageType.System,
                _ => MessageType.Assistant
            };

            // 获取文本内容
            if (string.IsNullOrWhiteSpace(msg.Text)) continue;
            
            session.AddMessage(msg.Text, msgType);
            hasNewMessage = true;
        }

        if (hasNewMessage)
        {
            repo.Update(session);
            await repo.SaveChangesAsync(cancellationToken);
        }
    }

    public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return JsonSerializer.SerializeToElement(_sessionSoreState);
    }
}