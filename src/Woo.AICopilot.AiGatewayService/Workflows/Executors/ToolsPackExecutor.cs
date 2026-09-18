using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using Woo.AICopilot.AgentPlugin;
using Woo.AICopilot.AiGatewayService.Models;

namespace Woo.AICopilot.AiGatewayService.Workflows.Executors;

public class ToolsPackExecutor(
    AgentPluginLoader pluginLoader,
    ILogger<ToolsPackExecutor> logger):
    Executor<List<IntentResult>>("ToolsPackExecutor")
{
    private const string ActionIntentPrefix = "Action.";
    
    public override async ValueTask HandleAsync(List<IntentResult> intentResults, IWorkflowContext context,
        CancellationToken ct = default)
    {
        try
        {
            // 1. 筛选工具类意图
            // 同样应用置信度过滤，避免误调用工具
            var actionIntents = intentResults
                .Where(i => i.Intent.StartsWith(ActionIntentPrefix, StringComparison.OrdinalIgnoreCase) 
                            && i.Confidence > 0.8) // 工具调用的风险较高，阈值设为 0.8 更安全
                .ToList();
            
            if (actionIntents.Count == 0)
            {
                // 在并行流中，没有工具意图直接返回空数组即可
                await context.SendMessageAsync(BranchResult.FromTools([]), ct);
                return;
            }

            logger.LogInformation("命中工具意图: {Intents}", string.Join(", ", actionIntents.Select(i => i.Intent)));
            
            // 2. 提取插件名称
            // 格式：Action.{PluginName} -> {PluginName}
            var pluginNames = actionIntents
                .Select(i => i.Intent.Substring(ActionIntentPrefix.Length))
                .Distinct()
                .ToArray();
            
            // 3. 动态加载工具
            // 利用 AgentPluginLoader 的能力，一次性获取所有相关插件的工具定义
            var tools = pluginLoader.GetAITools(pluginNames);
            
            logger.LogInformation("已加载 {Count} 个工具函数。", tools.Length);
            
            await context.SendMessageAsync(BranchResult.FromTools(tools), ct);
        }
        catch (Exception e)
        {
            logger.LogError(e, "加载工具集时发生错误");
            // 发生错误时，为了不熔断整个对话流程，可以选择降级处理：返回空工具集
            // 并通过 Context 发送一个警告事件（可选）
            await context.AddEventAsync(new ExecutorFailedEvent(Id, e), ct);
            await context.SendMessageAsync(BranchResult.FromTools([]), ct);
        }
    }
}