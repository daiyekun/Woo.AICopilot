using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Woo.AICopilot.AiGatewayService.Agents;
using Woo.AICopilot.AiGatewayService.Models;
using Woo.AICopilot.AiGatewayService.Queries.Sessions;

namespace Woo.AICopilot.AiGatewayService.Workflows.Executors;

public class IntentRoutingExecutor(
    IMediator mediator,
    IntentRoutingAgentBuilder agentBuilder, 
    ILogger<IntentRoutingExecutor> logger) :
    Executor<ChatStreamRequest>("IntentRoutingExecutor")
{
    public override async ValueTask HandleAsync(ChatStreamRequest request, IWorkflowContext context,
        CancellationToken cancellationToken = new())
    {
        try
        {
            logger.LogInformation("开始意图识别流程，SessionId: {SessionId}", request.SessionId);
            
            // 1. 更新工作流状态
            await context.QueueStateUpdateAsync("ChatStreamRequest", request, "Chat", cancellationToken: cancellationToken);
            
            // 2. 构建对话历史上下文
            // 我们不仅需要当前那句话，还需要之前的对话历史来辅助判断意图
            // 例如用户说“它多少钱？”，如果没有上文，意图无法识别。
            var result = await mediator.Send(new GetListChatMessagesQuery(request.SessionId, 4), cancellationToken);
            var history = result.Value!;
        
            history.Add(new ChatMessage(ChatRole.User, request.Message));
        
            // 3. 构建并运行 Agent
            var agent = await agentBuilder.BuildAsync();
            var response = await agent.RunAsync(
                history,
                cancellationToken: cancellationToken);
            
            // 记录原始响应，用于调试 Prompt 效果
            logger.LogDebug("意图识别原始响应: {ResponseText}", response.Text);
            
            // 将 Agent 的响应作为事件记录到工作流日志中
            await context.AddEventAsync(new AgentRunResponseEvent(Id, response), cancellationToken);
            
            // 4. 解析结果
            List<IntentResult> intentResults;
            try
            {
                intentResults = response.Deserialize<List<IntentResult>>(JsonSerializerOptions.Web);
            }
            catch (JsonException)
            {
                // 容错处理：如果解析失败，回退到兜底意图
                logger.LogWarning("意图识别 JSON 解析失败，回退到 General.Chat。原始文本: {Text}", response.Text);
                intentResults = [ new IntentResult { Intent = "General.Chat", Confidence = 1.0, Reasoning = "JSON解析失败" } ];
            }

            await context.SendMessageAsync(intentResults, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "意图识别节点发生严重错误");
            await context.AddEventAsync(new ExecutorFailedEvent(Id, e), cancellationToken);
            throw;
        }
    }
}