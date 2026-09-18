using System;
using System.Collections.Generic;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
#pragma warning disable MEAI001

namespace Woo.AICopilot.AiGatewayService.Workflows;

public class FinalAgentContext
{
    // 核心 Agent 实例
    public required AIAgent Agent { get; init; }
    
    // 当前对话的线程/历史记录
    public required AgentThread Thread { get; init; }
    
    // 用户输入的文本（或是经过 RAG 增强后的 Prompt）
    public required string InputText { get; set; }
    
    // 运行选项，包含了动态挂载的工具列表、温度设置等
    public required ChatClientAgentRunOptions RunOptions { get; init; }
    
    // 会话 ID
    public Guid SessionId { get; init; }
    
    // --- 审批相关状态 ---
    
    // 待处理的审批请求内容集合
    // 当 Agent 发起审批时，我们将请求对象暂存在这里
    public List<FunctionApprovalRequestContent> FunctionApprovalRequestContents { get; } = [];
    
    // 用户本次批准的 CallId 列表
    // 当用户提交批准时，前端会传回这些 ID
    public List<string> FunctionApprovalCallIds { get; } = [];
}