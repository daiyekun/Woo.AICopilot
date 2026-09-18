using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Woo.AICopilot.AiGatewayService.Agents;
using Woo.AICopilot.AiGatewayService.Models;
using Woo.AICopilot.DataAnalysisService.Services;
using Woo.AICopilot.Services.Common.Contracts;
using Woo.AICopilot.Services.Common.Helper;
using Woo.AICopilot.Visualization;
using Woo.AICopilot.Visualization.Widgets;
using VisualDecisionDto = Woo.AICopilot.DataAnalysisService.Plugins.VisualDecisionDto;

namespace Woo.AICopilot.AiGatewayService.Workflows.Executors;

/// <summary>
/// 数据分析执行器
/// 职责：处理 Analysis.* 意图，实例化 DBA Agent，执行 Text-to-SQL 任务。
/// </summary>
public class DataAnalysisExecutor(
    DataAnalysisAgentBuilder agentBuilder,
    IDataQueryService dataQuery,
    VisualizationContext vizContext,
    ILogger<DataAnalysisExecutor> logger) :
    Executor<List<IntentResult>>("DataAnalysisExecutor")
{
    private const string AnalysisIntentPrefix = "Analysis.";

    public override async ValueTask HandleAsync(
        List<IntentResult> intentResults, 
        IWorkflowContext context,
        CancellationToken ct = default)
    {
        // 1. 筛选数据分析类意图
        // 过滤规则：必须以 Analysis. 开头，且置信度高于 0.6
        var analysisIntents = intentResults
            .Where(i => i.Intent.StartsWith(AnalysisIntentPrefix, StringComparison.OrdinalIgnoreCase)
                        && i.Confidence > 0.6)
            .ToList();

        if (analysisIntents.Count == 0)
        {
            logger.LogDebug("未检测到数据分析意图，跳过执行。");
            // 返回空结果，表示该分支无产出
            await context.SendMessageAsync(BranchResult.FromDataAnalysis(string.Empty), ct);
            return;
        }

        logger.LogInformation("启动数据分析流程，命中目标数据库数量: {Count}", analysisIntents.Count);
        
        // 2. 遍历处理每一个意图
        var output = new StringBuilder();
        foreach (var intent in analysisIntents)
        {
            output.AppendLine(await ProcessSingleIntentAsync(intent, context, ct));
        }
        await context.SendMessageAsync(BranchResult.FromDataAnalysis(output.ToString()), ct);
    }
    
    /// <summary>
    /// 处理单个数据库查询意图
    /// </summary>
    private async Task<string> ProcessSingleIntentAsync(
        IntentResult intent, 
        IWorkflowContext context,
        CancellationToken ct)
    {
        var dbName = intent.Intent.Substring(AnalysisIntentPrefix.Length);
        
        try
        {
            // 1. 获取数据库配置
            // 我们需要 BusinessDatabase 实体来决定方言策略
            var db = await dataQuery.FirstOrDefaultAsync(
                dataQuery.BusinessDatabases.Where(d => d.Name == dbName));

            if (db == null || !db.IsEnabled)
            {
                logger.LogWarning("意图指向数据库 '{DbName}'，但该库不存在或已禁用。", dbName);
                return $"[系统提示]: 无法连接数据库 {dbName}，请联系管理员核实配置。";
            }

            // 2. 构建 DBA Agent
            // 这里会动态注入 PG 或 SQLServer 的方言提示词
            var agent = await agentBuilder.BuildAsync(db);
            // 创建临时会话线程
            var thread = agent.GetNewThread();
            
            // 4. 执行 ReAct 循环
            // Agent 会自动进行: 思考 -> GetTableNames -> 思考 -> GetTableSchema -> 思考 -> ExecuteSQL -> 总结
            await foreach (var update in agent.RunStreamingAsync(intent.Query!, thread, cancellationToken: ct))
            {
                
                await context.AddEventAsync(new AgentRunUpdateEvent(Id, update), ct);
            }
            
            // 记录日志以便调试
            logger.LogInformation("数据库 {DbName} 查询完成。", dbName);
            
            // 获取可视化上下文
            var (rawData, schema) = vizContext.GetLastResult();
            var output = vizContext.GetOutput();
            
            // =========================================================
            // 分流路径 1：旁路输出 (Side Path) -> 前端 Widget
            // 目标：visual_decision + data -> Widget JSON
            // =========================================================
            if (output is { Decision: not null } && vizContext.HasData)
            {
                try
                {
                    var widget = BuildWidget(output.Decision, rawData!, schema!);
                    var message = new ChatMessage(ChatRole.Assistant, widget.ToJson());
                    await context.AddEventAsync(new AgentRunResponseEvent(Id, new AgentRunResponse(message)), ct);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "构建可视化 Widget 失败。Database: {DbName}", dbName);
                    return $"[系统错误]: 构建可视化 Widget 时发生异常 - {ex.Message}";
                }
            }
           
            // =========================================================
            // 分流路径 2：主路输出 (Main Path) -> 聚合器 -> Final Agent
            // 目标：schema + data -> Combined JSON
            // =========================================================
        
            // 这里直接使用匿名对象进行拼接：
            // { "schema": [], "data": [] }
            var combinedOutput = new
            {
                analysis = output.Analysis,         // 直接透传 Agent 生成的 Schema
                data = rawData ?? []              // 拼接 SQL 查询的实际结果
            };
            

            return combinedOutput.ToJson();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "执行数据分析意图失败。Database: {DbName}", dbName);
            return $"[系统错误]: 查询数据库 {dbName} 时发生异常 - {ex.Message}";
        }
    }
    
    private IWidget BuildWidget(VisualDecisionDto decision, IEnumerable<dynamic> data, IEnumerable<SchemaColumn> schema)
    {
        switch (decision.Type)
        {
            case WidgetType.StatsCard:
                // 取第一行第一列，或者根据列名查找
                var firstRow = data.First() as IDictionary<string, object>;
                var value = firstRow.Values.First(); // 简单粗暴取第一个值
            
                return new StatsCardWidget
                {
                    Title = decision.Title,
                    Description = decision.Description,
                    Data = new StatsCardData
                    {
                        Label = decision.Title,
                        Value = value,
                        Unit = decision.Unit
                    }
                };

            case WidgetType.DataTable:
                return new DataTableWidget
                {
                    Title = decision.Title,
                    Data = data.ToDataTableData(schema)
                };

            case WidgetType.Chart:
                var dataset = data.ToChartDataset(schema);
                return new ChartWidget
                {
                    Title = decision.Title,
                    Data = new ChartData
                    {
                        Category = decision.ChartConfig!.Category,
                        Dataset = dataset,
                        Encoding = new ChartEncoding
                        {
                            X = decision.ChartConfig.X,
                            Y = string.IsNullOrWhiteSpace(decision.ChartConfig.Y) 
                                ? []
                                : [decision.ChartConfig.Y],
                            SeriesName = decision.ChartConfig.Series
                        }
                    }
                };

            default:
                throw new NotSupportedException($"不支持的 Widget 类型: {decision.Type}");
        }
    }
    
}