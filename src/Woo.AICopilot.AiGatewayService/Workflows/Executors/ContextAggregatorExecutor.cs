using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using Woo.AICopilot.AiGatewayService.Agents;

namespace Woo.AICopilot.AiGatewayService.Workflows.Executors;

/// <summary>
/// 上下文聚合执行器
/// 职责：作为 Fan-in 节点，接收来自所有并行分支的 BranchResult。
/// 只有当接收到的结果数量达到预期（3个）时，才进行合并并触发下游。
/// </summary>
public class ContextAggregatorExecutor(ILogger<ContextAggregatorExecutor> logger) 
    : Executor<BranchResult>("ContextAggregatorExecutor")
{
    // 内部状态：用于跨方法调用累积结果
    private readonly List<BranchResult> _accumulatedResults = [];
    
    // 硬编码预期分支数：Tools + Knowledge + DataAnalysis = 3
    private const int ExpectedBranchCount = 3;

    public override async ValueTask HandleAsync(
        BranchResult branchResult, 
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        // 1. 累积状态
        // 注意：FanInEdge 可能一次性送来所有结果，也可能分批送来
        // 因此我们需要 AddRange 并检查总数
        _accumulatedResults.AddRange(branchResult);

        // 2. 完备性检查
        if (_accumulatedResults.Count >= ExpectedBranchCount)
        {
            logger.LogInformation("并行分支汇聚完成，开始合并上下文。");

            // 3. 恢复原始请求 (从全局状态中读取)
            var request = await context.ReadStateAsync<ChatStreamRequest>("ChatStreamRequest", "Chat", cancellationToken);
            if (request == null) throw new InvalidOperationException("无法获取原始会话请求");

            var genContext = new GenerationContext { Request = request };

            // 4. 合并数据
            foreach (var result in _accumulatedResults)
            {
                switch (result.Type)
                {
                    case BranchType.Tools when result.Tools != null:
                        genContext.Tools = result.Tools;
                        break;
                    case BranchType.Knowledge when !string.IsNullOrWhiteSpace(result.Knowledge):
                        genContext.KnowledgeContext = result.Knowledge;
                        break;
                    case BranchType.DataAnalysis when !string.IsNullOrWhiteSpace(result.DataAnalysis):
                        genContext.DataAnalysisContext = result.DataAnalysis;
                        break;
                }
            }

            // 5. 清理状态 (为可能的下一轮对话做准备)
            _accumulatedResults.Clear();

            // 6. 手动发送聚合结果消息，触发下游
            await context.SendMessageAsync(genContext, cancellationToken);
        }
        else
        {
            // 如果未满足条件，不返回/发送任何值/消息（Task完成但无Output），流程在此暂停等待下一批消息
            logger.LogDebug("聚合进度: {Current}/{Total}，等待其他分支...", _accumulatedResults.Count, ExpectedBranchCount);
        }
    }
}