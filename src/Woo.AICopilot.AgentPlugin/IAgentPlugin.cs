using Microsoft.Extensions.AI;

namespace Woo.AICopilot.AgentPlugin;

/// <summary>
/// 定义 Agent 插件的标准接口。
/// 所有希望被系统发现的工具集都必须实现此接口。
/// </summary>
public interface IAgentPlugin
{
    /// <summary>
    /// 插件的唯一标识名称（通常使用类名）。
    /// 用于在运行时筛选和查找插件。
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// 插件的功能描述。
    /// 可以提供给 Agent 进行“元认知”判断，决定是否需要使用此插件。
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// 获取该插件包含的所有 AI 工具定义（AITool）。
    /// 这些定义将被直接传递给 LLM。
    /// </summary>
    IEnumerable<AITool>? GetAITools();
    
    /// <summary>
    /// 获取该插件中被标记为“高风险”或“敏感”的工具名称列表。
    /// 位于此列表中的工具，在被 Agent 调用时，会触发人机回环拦截机制，
    /// 要求用户显式批准后方可执行。
    /// </summary>
    IEnumerable<string>? HighRiskTools { get; }
}