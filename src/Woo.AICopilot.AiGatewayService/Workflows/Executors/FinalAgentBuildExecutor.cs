using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Woo.AICopilot.AiGatewayService.Agents;
using Woo.AICopilot.Services.Common.Contracts;

#pragma warning disable MEAI001

namespace Woo.AICopilot.AiGatewayService.Workflows.Executors;

/// <summary>
/// 最终 Agent 构建执行器
/// 职责：利用聚合后的上下文构建 Agent，注入 RAG 提示词。
/// </summary>
public class FinalAgentBuildExecutor(
    ChatAgentFactory agentFactory, 
    IDataQueryService dataQuery,
    ILogger<FinalAgentBuildExecutor> logger):
    Executor<GenerationContext>("FinalAgentBuildExecutor")
{
    public override async ValueTask HandleAsync(
        GenerationContext genContext, 
        IWorkflowContext context,
        CancellationToken ct = default)
    {
        try
        {
            var request = genContext.Request;
            logger.LogInformation("开始最终生成，SessionId: {SessionId}", request.SessionId);

            // 1. 获取会话关联的模板配置
            // 我们需要知道当前会话使用的是哪个 Agent 模板（例如"通用助手"或"HR助手"）
            var session = await dataQuery.FirstOrDefaultAsync(dataQuery.Sessions.Where(s => s.Id == request.SessionId));
            
            if (session == null) throw new InvalidOperationException("会话不存在");

            // 2. 创建基础 Agent 实例
            // 此时 Agent 拥有的是数据库中定义的静态 System Prompt
            var agent = await agentFactory.CreateAgentAsync(session.TemplateId, isSaveChatMessage: false);
            
            // 3. 构建 Prompt (RAG 与 数据分析上下文注入)
            string finalUserPrompt;
            var hasKnowledge = !string.IsNullOrWhiteSpace(genContext.KnowledgeContext);
            var hasDataAnalysis = !string.IsNullOrWhiteSpace(genContext.DataAnalysisContext);
            var hasContext = hasKnowledge || hasDataAnalysis;
            
            if (hasContext)
            {
                // 构建混合上下文内容
                var contextBuilder = new StringBuilder();

                if (hasDataAnalysis)
                {
                    
                    contextBuilder.AppendLine("数据库查询结果：");
                    contextBuilder.AppendLine(genContext.DataAnalysisContext);
                    contextBuilder.AppendLine();
                }

                if (hasKnowledge)
                {
                    contextBuilder.AppendLine("知识库检索参考信息：");
                    contextBuilder.AppendLine(genContext.KnowledgeContext);
                    contextBuilder.AppendLine();
                }
                
                finalUserPrompt = $"""
                                   请基于以下参考信息（包含数据库查询结果或检索文档）回答我的问题：

                                   <context>
                                   {contextBuilder}
                                   </context>

                                   回答要求：
                                   1. 引用参考信息时，请标注来源 ID（例如 [^1]）。
                                   2. 针对数据分析结果，请结合用户问题进行自然语言解释。
                                   3. 在回答结尾，如果引用了知识库文档，请生成“参考资料”列表。
                                   4. 如果参考信息不足以回答问题，请直接说明，严禁编造。
                                   5. 保持回答专业、简洁。

                                   我的问题：
                                   {request.Message}
                                   """;
                
                logger.LogDebug("增强模式激活：注入知识({KSize})，注入数据({DSize})。", 
                    genContext.KnowledgeContext?.Length ?? 0, 
                    genContext.DataAnalysisContext?.Length ?? 0);
            }
            else
            {
                // 无上下文模式：直接透传用户问题
                finalUserPrompt = request.Message;
                logger.LogDebug("增强模式未激活：仅使用用户原始输入。");
            }

            // 4. 准备执行参数 (ChatOptions)
            // 将动态加载的工具集挂载到本次执行的选项中
            var runOptions = new ChatClientAgentRunOptions
            {
                ChatOptions = new ChatOptions
                {
                    Tools = genContext.Tools // <-- 动态挂载工具
                }
            };

            // 如果有注入任何上下文（知识或数据），都降低温度以保证事实性
            if (hasContext)
            {
                runOptions.ChatOptions.Temperature = 0.3f;
            }
            

            // 5. 构建 FinalAgentContext 并传递给下一个节点
            // 注意：我们这里不执行 RunStreamingAsync，而是创建好环境就交棒。
            var agentThread = agent.GetNewThread();
            var finalAgentContext = new FinalAgentContext
            {
                Agent = agent,
                Thread = agentThread,
                InputText = finalUserPrompt,
                RunOptions = runOptions,
                SessionId = request.SessionId
            };
            
            // 将构建好的 Context 发送给工作流的下一个节点 (即 FinalAgentRunExecutor)
            await context.SendMessageAsync(finalAgentContext, ct);
        }
        catch (Exception e)
        {
            logger.LogError(e, "最终 Agent 构建阶段发生错误");
            await context.AddEventAsync(new ExecutorFailedEvent(Id, e), ct);
            throw;
        }
    }
}