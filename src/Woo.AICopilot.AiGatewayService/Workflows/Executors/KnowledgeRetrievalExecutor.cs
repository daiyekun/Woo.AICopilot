using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using Woo.AICopilot.AiGatewayService.Models;
using Woo.AICopilot.RagService.Queries.KnowledgeBases;
using Woo.AICopilot.Services.Common.Contracts;

namespace Woo.AICopilot.AiGatewayService.Workflows.Executors;

/// <summary>
/// 知识检索执行器
/// 职责：解析知识意图，并行检索向量数据库，并生成带有引用源的上下文文本。
/// </summary>
public class KnowledgeRetrievalExecutor(
    IMediator mediator,
    IDataQueryService dataQueryService,
    ILogger<KnowledgeRetrievalExecutor> logger) : 
    Executor<List<IntentResult>>("KnowledgeRetrievalExecutor")
{
    // 定义意图前缀常量，与 Prompt 中的定义保持一致
    private const string KnowledgeIntentPrefix = "Knowledge.";

    public override async ValueTask HandleAsync(
        List<IntentResult> intentResults, 
        IWorkflowContext context,
        CancellationToken ct = default)
    {
      
        // 1. 筛选知识类意图
        // 过滤掉置信度不足或非知识类的意图
        var knowledgeIntents = intentResults
            .Where(i => i.Intent.StartsWith(KnowledgeIntentPrefix, StringComparison.OrdinalIgnoreCase) 
                        && i.Confidence > 0.6) // 0.6 为硬编码的最低置信度阈值，防止低质量检索
            .ToList();

        if (knowledgeIntents.Count == 0)
        {
            logger.LogDebug("未检测到知识库意图，跳过检索流程。");
            await context.SendMessageAsync(BranchResult.FromKnowledge(string.Empty), ct);
            return;
        }

        logger.LogInformation("开始执行知识检索，命中意图数量: {Count}", knowledgeIntents.Count);

        // 2. 解析知识库名称并获取对应的 ID
        // 意图格式：Knowledge.{Name}，我们需要提取 {Name}
        var kbNames = knowledgeIntents
            .Select(i => i.Intent.Substring(KnowledgeIntentPrefix.Length))
            .Distinct()
            .ToList();

        // 从数据库中批量查询 KnowledgeBaseId
        var knowledgeBases = await dataQueryService.ToListAsync(
            dataQueryService.KnowledgeBases.Where(kb => kbNames.Contains(kb.Name))
        );

        if (knowledgeBases.Count == 0)
        {
            logger.LogWarning("意图命中了知识库名称 {Names}，但在数据库中未找到对应配置。", string.Join(", ", kbNames));
            await context.SendMessageAsync(BranchResult.FromKnowledge(string.Empty), ct);
            return;
        }

        // 3. 构建并执行并行检索任务
        // 针对每个有效的意图，构建一个 SearchKnowledgeBaseQuery
        var searchTasks = new List<Task<string>>();

        foreach (var intent in knowledgeIntents)
        {
            // 提取知识库名称
            var kbName = intent.Intent.Substring(KnowledgeIntentPrefix.Length);
            var kb = knowledgeBases.FirstOrDefault(k => k.Name.Equals(kbName, StringComparison.OrdinalIgnoreCase));

            if (kb == null) continue;

            // 如果为空则跳过
            if (string.IsNullOrWhiteSpace(intent.Query))
            {
                logger.LogWarning("意图 {Intent} 缺少查询关键词，跳过。", intent.Intent);
                continue;
            }

            // 创建异步任务
            searchTasks.Add(ExecuteSearchAsync(kb.Id, kb.Name, intent.Query, ct));
        }

        if (searchTasks.Count == 0)
        {
            await context.SendMessageAsync(BranchResult.FromKnowledge(string.Empty), ct);
            return;
        }

        // 并行等待所有检索完成
        var searchResults = await Task.WhenAll(searchTasks);

        // 4. 聚合与格式化输出
        // 将所有任务返回的 Markdown 片段拼接在一起
        var combinedContext = string.Join("\n\n", searchResults.Where(s => !string.IsNullOrWhiteSpace(s)));
        
        await context.SendMessageAsync(BranchResult.FromKnowledge(combinedContext), ct);
    }

    /// <summary>
    /// 执行单个知识库的检索并格式化结果
    /// </summary>
    private async Task<string> ExecuteSearchAsync(
        Guid kbId, 
        string kbName, 
        string queryText, 
        CancellationToken ct)
    {
        try
        {
            // 调用 RagService 的 SearchKnowledgeBaseQuery
            // TopK=3, MinScore=0.5 是经验参数，可以根据业务需求调整
            var query = new SearchKnowledgeBaseQuery(kbId, queryText, TopK: 3, MinScore: 0.5);
            var result = await mediator.Send(query, ct);

            if (!result.IsSuccess || result.Value == null)
            {
                return string.Empty;
            }

            // 格式化为 XML 引用块
            var sb = new StringBuilder();
            foreach (var item in result.Value)
            {
                // 使用 <document> 标签包裹内容
                // 将元数据（ID、名称、分数）作为 XML 属性
                sb.AppendLine($"<document id=\"{item.DocumentId}\" name=\"{item.DocumentName}\" score=\"{item.Score:F2}\">");
                
                // 直接填充原始内容
                // 这样无论内容是 Markdown 表格、代码块还是标题，都被限制在 document 标签内部
                sb.AppendLine(item.Text);
                
                sb.AppendLine("</document>");
                sb.AppendLine(); 
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "检索知识库 {KbName} 时发生异常。", kbName);
            return string.Empty;
        }
    }
}