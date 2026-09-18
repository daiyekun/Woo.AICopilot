using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using Woo.AICopilot.AgentPlugin;
using Woo.AICopilot.Core.McpServer.Aggregates.McpServerInfo;
using Woo.AICopilot.Services.Common.Contracts;
#pragma warning disable MEAI001

namespace Woo.AICopilot.McpService;

public class McpServerBootstrap(
    IDataQueryService dataQueryService,
    AgentPluginLoader agentPluginLoader,
    ILogger<McpServerBootstrap> logger)
    : IMcpServerBootstrap
{
    public async IAsyncEnumerable<McpClient> StartAsync([EnumeratorCancellation] CancellationToken ct)
    {
        var query = dataQueryService.McpServerInfos
            .Where(m => m.IsEnabled);

        var mcpServerInfos = await dataQueryService.ToListAsync(query);

        foreach (var mcpServerInfo in mcpServerInfos)
        {
            McpClient mcpClient = null!;
            switch (mcpServerInfo.TransportType)
            {
                case McpTransportType.Stdio:
                    mcpClient = await CreateStdioClientAsync(mcpServerInfo, ct);
                    break;
                case McpTransportType.Sse:
                    mcpClient = await CreateSseClientAsync(mcpServerInfo, ct);
                    break;
            }

            logger.LogInformation(
                "已连接到 MCP 服务器 - {Name}",
                mcpServerInfo.Name);

            var tools = await mcpClient.ListToolsAsync(
                cancellationToken: ct);

            logger.LogInformation(
                "已发现 {ToolsCount} 个工具",
                tools.Count);
            
            // 2. 构建并注册适配器插件
            // 这一步将 MCP 的数据模型转换为 Agent 的插件模型
            RegisterMcpPlugin(mcpServerInfo, tools);
            
            logger.LogInformation(
                "已注册 MCP 插件 - {Name}",
                mcpServerInfo.Name);
            
            yield return mcpClient;
        }
    }
    
    /// <summary>
    /// 将 MCP 服务元数据和工具列表封装为通用桥接插件，并注册到系统。
    /// 在此过程中，根据配置动态注入审批拦截器。
    /// </summary>
    /// <param name="mcpServerInfo">包含敏感工具配置的数据库实体</param>
    /// <param name="mcpTools">从 MCP Client 实时获取的原始工具列表</param>
    private void RegisterMcpPlugin(McpServerInfo mcpServerInfo, IList<McpClientTool> mcpTools)
    {
        // 1. 动态转换与封装工具
        var tools = mcpTools
            .Select<McpClientTool, AIFunction>(tool =>
            {
                // 步骤 A: 检查当前工具是否在数据库配置的敏感列表中
                var isSensitive = mcpServerInfo.SensitiveTools != null &&
                                  mcpServerInfo.SensitiveTools.Contains(tool.Name);

                if (!isSensitive)
                {
                    // 如果不是敏感工具，直接返回原始的 McpClientTool
                    // McpClientTool 本身实现了 AIFunction，可以直接使用
                    return tool;
                }

                // 步骤 B: 注入审批拦截器
                // 对于 MCP 工具，原理与原生插件完全一致。
                // 我们使用 ApprovalRequiredAIFunction 将原始的 MCP 工具包裹起来。
                // 当 Agent 调用此工具时，会先触发宿主的审批流程，
                // 审批通过后，ApprovalRequiredAIFunction 内部会调用 tool.InvokeAsync，
                // 进而通过 JSON-RPC 发送给远程的 Node.js 进程。
                var approvalFunction = new ApprovalRequiredAIFunction(tool);
                return approvalFunction;
            });

        // 2. 创建通用桥接插件
        // 将处理过的工具列表（包含普通工具和包装后的审批工具）赋值给插件
        var mcpPlugin = new GenericBridgePlugin
        {
            Name = mcpServerInfo.Name,
            Description = mcpServerInfo.Description,
            // 这里传入的是已经混合了 Wrapper 的 AIFunction 集合
            AITools = tools, 
            
            // 同时，我们将原始的敏感列表赋值给 HighRiskTools 属性
            // 这样做是为了让 UI 层或元数据层能够知道哪些工具是高风险的
            // （即使执行层的拦截已经由 AITools 内部的对象处理了）
            HighRiskTools = mcpServerInfo.SensitiveTools
        };

        // 3. 注册到全局插件加载器
        agentPluginLoader.RegisterAgentPlugin(mcpPlugin);
    }
    private async Task<McpClient> CreateStdioClientAsync(McpServerInfo mcpServerInfo, CancellationToken ct)
    {
        var transportOptions = new StdioClientTransportOptions
        {
            Command = "npx",
            Arguments = mcpServerInfo.Arguments.Split(' ')
        };

        var transport = new StdioClientTransport(transportOptions);
        return await McpClient.CreateAsync(
            transport,
            cancellationToken: ct);
    }
    
    private async Task<McpClient> CreateSseClientAsync(McpServerInfo mcpServerInfo, CancellationToken ct)
    {
        var transportOptions = new HttpClientTransportOptions
        {
            Endpoint = new Uri(mcpServerInfo.Arguments)
        };

        var transport = new HttpClientTransport(transportOptions);
        return await McpClient.CreateAsync(
            transport,
            cancellationToken: ct);
    }
}