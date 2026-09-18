using Microsoft.Extensions.AI;

namespace Woo.AICopilot.AgentPlugin;

/// <summary>
/// MCP 通用桥接插件。
/// 该类实现了 IAgentPlugin 接口，用于将外部 MCP 服务适配为内部的原生插件。
/// 它不包含具体的业务逻辑，而是作为 MCP 工具集的容器。
/// </summary>
public class GenericBridgePlugin : IAgentPlugin
{
    /// <summary>
    /// 插件名称。
    /// 映射自 McpServerInfo.Name。
    /// 使用 required 关键字强制在初始化时赋值，确保插件标识的完整性。
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 插件描述。
    /// 映射自 McpServerInfo.Description。
    /// 这段描述将被注入到 LLM 的 System Prompt 中，用于指导模型何时使用该插件。
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// 动态注入的工具列表。
    /// 这些工具对象由 MCP SDK 在握手阶段解析生成，包含了工具名称、参数 Schema 以及执行回调。
    /// </summary>
    public IEnumerable<AITool>? AITools { get; init; }

    /// <summary>
    /// 实现接口方法，向 Agent 暴露该插件所拥有的所有能力。
    /// </summary>
    /// <returns>工具定义集合</returns>
    public IEnumerable<AITool>? GetAITools()
    {
        return AITools;
    }

    public IEnumerable<string>? HighRiskTools { get; init; }
}