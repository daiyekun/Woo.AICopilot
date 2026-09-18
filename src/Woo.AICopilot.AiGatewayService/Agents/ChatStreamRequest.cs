using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using MediatR;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Woo.AICopilot.AiGatewayService.Models;
using Woo.AICopilot.AiGatewayService.Workflows;
using Woo.AICopilot.Services.Common.Attributes;
using Woo.AICopilot.Services.Common.Contracts;
using Woo.AICopilot.Services.Common.Helper;
#pragma warning disable MEAI001

namespace Woo.AICopilot.AiGatewayService.Agents;

[AuthorizeRequirement("AiGateway.Chat")]
// CallId 列表：如果不为空，表示这是一次针对特定工具调用的审批响应
public record ChatStreamRequest(Guid SessionId, string Message, List<string>? CallIds) : IStreamRequest<ChatChunk>;

public class ChatStreamHandler(
    IDataQueryService queryService, 
    WorkflowFactory workflowFactory) 
    : IStreamRequestHandler<ChatStreamRequest, ChatChunk>
{
    // 内存状态存储：SessionId -> 挂起的 AgentContext
    private static readonly Dictionary<Guid, FinalAgentContext> AgentContexts = new();

    public async IAsyncEnumerable<ChatChunk> Handle(ChatStreamRequest request, [EnumeratorCancellation] CancellationToken ct)
    {
        // 1. 基础校验
        if (!queryService.Sessions.Any(session => session.Id == request.SessionId))
        {
            throw new Exception("未找到会话");
        }
        
        // 2. 路由判断：是审批响应还是新对话？
        if (request.CallIds != null && request.CallIds.Count != 0)
        {
            // --- 分支 A：处理审批响应 ---
            
            // 尝试从内存中取出之前挂起的 Context
            AgentContexts.TryGetValue(request.SessionId, out var agentContext);
            if (agentContext == null)
            {
                throw new Exception("会话已过期或上下文丢失，无法完成审批流程。");
            }

            // 更新 Context 状态
            agentContext.InputText = request.Message; // "批准" 或 "拒绝"
            agentContext.FunctionApprovalCallIds.AddRange(request.CallIds); // 用户批准的 ID 列表
            
            // 创建仅包含 AgentRun 阶段的短工作流
            // 我们不需要重新执行 Build，直接复用现有的 AgentContext
            var workflow = workflowFactory.CreateFinalAgentRunWorkflow();
            
            // 启动工作流（传入AgentContext）
            await using var workflowRun = await InProcessExecution.StreamAsync(workflow, agentContext, cancellationToken: ct);
            
            // 监听并转发事件
            await foreach (var chatChunk in RunWorkflow(workflowRun, request.SessionId, ct))
            {
                yield return chatChunk;
            }

            // 流程结束后，如果所有审批请求都处理完了，就可以移除缓存
            if (agentContext.FunctionApprovalRequestContents.Count == 0)
            {
                AgentContexts.Remove(request.SessionId);
            }
        }
        else
        {
            // --- 分支 B：处理新对话 ---
            
            // 创建完整的意图识别工作流 (Intent -> ... -> Build -> Run)
            var workflow = workflowFactory.CreateIntentWorkflow();
            
            // 启动工作流（传入用户请求）
            await using var workflowRun = await InProcessExecution.StreamAsync(workflow, request, cancellationToken: ct);
            
            // 监听并转发事件
            await foreach (var chatChunk in RunWorkflow(workflowRun, request.SessionId, ct))
            {
                yield return chatChunk;
            };
        }
    }

    // 事件转换逻辑：将工作流事件转换为前端可消费的 ChatChunk
    // [增加] 监听函数审批请求对象
    private async IAsyncEnumerable<ChatChunk> RunWorkflow(StreamingRun workflowRun, Guid sessionId, CancellationToken ct)
    {
        await foreach (var workflowEvent in workflowRun.WatchStreamAsync(ct))
        {
            Console.WriteLine(workflowEvent);
            switch (workflowEvent)
            {
                case WorkflowOutputEvent evt:
                    if (evt.Data is FinalAgentContext agentContext && agentContext.FunctionApprovalRequestContents.Count != 0)
                    {
                        AgentContexts.TryAdd(sessionId, agentContext);
                    }
                    break;
                case ExecutorFailedEvent evt:
                    yield return new ChatChunk(evt.ExecutorId, ChunkType.Error, evt.Data?.Message ?? string.Empty);
                    break;
                case AgentRunResponseEvent evt:
                    switch (evt.ExecutorId)
                    {
                        case "IntentRoutingExecutor":
                            yield return new ChatChunk(evt.ExecutorId, ChunkType.Intent, evt.Response.Text);
                            break;
                        case "DataAnalysisExecutor":
                            yield return new ChatChunk(evt.ExecutorId, ChunkType.Widget, evt.Response.Text);
                            break;
                    }
                    break;
                case AgentRunUpdateEvent evt:
                    foreach (var evtContent in evt.Update.Contents)
                    {
                        switch (evtContent)
                        {
                            case TextContent content:
                                yield return new ChatChunk(evt.ExecutorId, ChunkType.Text, content.Text);
                                break;
                            case FunctionCallContent content:
                                var fun = new
                                {
                                    id = content.CallId,
                                    name = content.Name, 
                                    args = content.Arguments
                                };
                                yield return new ChatChunk(evt.ExecutorId, ChunkType.FunctionCall, fun.ToJson());
                                break;
                            case FunctionResultContent content:
                                var result = new
                                {
                                    id = content.CallId,
                                    result = content.Result
                                };
                                yield return new ChatChunk(evt.ExecutorId, ChunkType.FunctionResult,
                                    result.ToJson());
                                break;
                            case FunctionApprovalRequestContent content:
                                // 监听函数审批请求对象
                                var approval = new
                                {
                                    callId = content.FunctionCall.CallId,
                                    name = content.FunctionCall.Name,
                                    args = content.FunctionCall.Arguments
                                };
                                yield return new ChatChunk(evt.ExecutorId, ChunkType.ApprovalRequest,
                                    approval.ToJson());
                                break;
                        }
                    }
                    break;
            }
        }
    }
}