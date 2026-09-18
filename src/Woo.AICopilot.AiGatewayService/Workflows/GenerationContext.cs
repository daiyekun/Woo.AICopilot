using Microsoft.Extensions.AI;
using Woo.AICopilot.AiGatewayService.Agents;

namespace Woo.AICopilot.AiGatewayService.Workflows;

/// <summary>
/// 生成上下文（聚合后的最终对象）
/// 职责：包含生成回答所需的所有素材，由聚合器构建并传递给最终 Agent。
/// </summary>
public class GenerationContext
{
    /// <summary>
    /// 原始请求信息
    /// </summary>
    public required ChatStreamRequest Request { get; init; }

    /// <summary>
    /// 聚合后的工具集
    /// </summary>
    public AITool[] Tools { get; set; } = [];

    /// <summary>
    /// 聚合后的知识上下文
    /// </summary>
    public string KnowledgeContext { get; set; } = string.Empty;
    
    /// <summary>
    /// 聚合后的数据分析上下文
    /// </summary>
    public string DataAnalysisContext { get; set; } = string.Empty;
}