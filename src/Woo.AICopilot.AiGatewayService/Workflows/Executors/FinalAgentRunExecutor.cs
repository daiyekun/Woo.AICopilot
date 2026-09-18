using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

#pragma warning disable MEAI001

namespace Woo.AICopilot.AiGatewayService.Workflows.Executors;

/// <summary>
/// Agent 流式运行执行器
/// 职责：执行对话循环，处理审批请求拦截与响应恢复。
/// </summary>
public class FinalAgentRunExecutor(
    ILogger<FinalAgentRunExecutor> logger):
    Executor<FinalAgentContext, FinalAgentContext>("FinalAgentRunExecutor")
{
    public override async ValueTask<FinalAgentContext> HandleAsync(
        FinalAgentContext agentContext, 
        IWorkflowContext context,
        CancellationToken cancellationToken = new())
    {
        try
        {
            // 1. 构建本次发送给 Agent 的消息列表
            List<ChatMessage> message = [];
            
            // 检查是否存在待处理的“批准 CallId”
            // 如果存在，说明这是审批后的恢复流程，而不是新的一轮对话
            var isApprovalResumption = agentContext.FunctionApprovalRequestContents.Count != 0 
                                        && agentContext.FunctionApprovalCallIds.Count != 0;
            
            if (isApprovalResumption)
            {
                // --- 审批恢复逻辑 ---
                logger.LogInformation("检测到审批响应，正在恢复 Agent 执行...");
                
                foreach (var callId in agentContext.FunctionApprovalCallIds)
                {
                    // 在暂存的请求列表中查找对应的 RequestContent
                    var requestContent = agentContext.FunctionApprovalRequestContents
                        .FirstOrDefault(rc => rc.FunctionCall.CallId == callId);
                    if (requestContent == null)
                    {
                        logger.LogWarning("未找到 CallId: {CallId} 的审批请求上下文，跳过。", callId);
                        continue;
                    };
                    
                    // 核心逻辑：模拟生成审批结果消息
                    // CreateResponse 是框架提供的方法，它会为特定的审批请求生成一个审批响应对象：
                    // True 表示通过审批，False 表示未通过审批
                    var isApproved = agentContext.InputText == "批准"; 
                    var response = requestContent.CreateResponse(isApproved);
                    
                    // 将这个响应包装为 User 消息发送给 Agent
                    // Agent 收到后，内部机制会解除挂起状态，真正执行工具调用
                    message.Add(new ChatMessage(ChatRole.User,[response]));
                    
                    // 清理已处理的请求
                    agentContext.FunctionApprovalRequestContents.Remove(requestContent);
                }
                
                // 清空本次处理的 ID 列表
                agentContext.FunctionApprovalCallIds.Clear();

            } else {
                // --- 正常对话逻辑 ---
                // 直接发送用户的 Prompt
                message.Add(new ChatMessage(ChatRole.User, agentContext.InputText));
            }
            
            // 2. 进入流式运行循环
            // 无论是初次运行还是恢复运行，都复用同一个 agentContext.Thread 和 RunOptions
            await foreach (var update in agentContext.Agent.RunStreamingAsync(
                               message,
                               agentContext.Thread,
                               agentContext.RunOptions,
                               cancellationToken))
            {
                // 3. 实时捕获流中的内容
                foreach (var content in update.Contents)
                {
                    // 关键点：拦截审批请求
                    // 如果 Agent 想要执行敏感操作，它不会直接执行，而是产生 FunctionApprovalRequestContent
                    if (content is FunctionApprovalRequestContent requestContent)
                    {
                        logger.LogInformation("Agent 发起审批请求: {Name}", requestContent.FunctionCall.Name);
                        // 我们必须将这个请求暂存到 Context 中，以便后续恢复时使用
                        agentContext.FunctionApprovalRequestContents.Add(requestContent);
                    }
                };
                
                // 4. 将更新事件转发给工作流，最终推送给前端
                await context.AddEventAsync(new AgentRunUpdateEvent(Id, update), cancellationToken);
            }
            
            // 返回更新后的 Context，以便状态保持
            return agentContext;
        }
        catch (Exception e)
        {
            logger.LogError(e, "最终Agent运行阶段发生错误");
            await context.AddEventAsync(new ExecutorFailedEvent(Id, e), cancellationToken);
            throw;
        }
    }
}