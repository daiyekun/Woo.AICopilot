using Microsoft.Agents.AI.Workflows;
using Woo.AICopilot.AiGatewayService.Workflows.Executors;

namespace Woo.AICopilot.AiGatewayService.Workflows;

public class WorkflowFactory(
    IntentRoutingExecutor intentRouting,
    ToolsPackExecutor toolsPack,
    KnowledgeRetrievalExecutor knowledgeRetrieval,
    DataAnalysisExecutor dataAnalysis,
    ContextAggregatorExecutor contextAggregator,
    FinalAgentBuildExecutor agentBuild,
    FinalAgentRunExecutor agentRun)
{
    public Workflow CreateIntentWorkflow()
    {
        var workflowBuilder = new WorkflowBuilder(intentRouting)
            // 1. 扇出 (Fan-out): 意图识别 -> [工具打包, 知识检索]
            // IntentRoutingExecutor 输出的 List<IntentResult> 会被广播给 targets 列表中的每一个节点
            .AddFanOutEdge(intentRouting, [toolsPack, knowledgeRetrieval, dataAnalysis])
            // 2. 扇入 (Fan-in): [工具打包, 知识检索] -> 聚合器
            // 聚合器接收来自 sources 列表的所有输出
            .AddFanInEdge([toolsPack, knowledgeRetrieval, dataAnalysis], contextAggregator)
            // 3. 线性连接: 聚合器 -> 最终处理
            .AddEdge(contextAggregator, agentBuild)
            .AddEdge(agentBuild, agentRun)
            .WithOutputFrom(agentRun);
            
        return workflowBuilder.Build();
    }
    
    public Workflow CreateFinalAgentRunWorkflow()
    {
        var workflowBuilder = new WorkflowBuilder(agentRun);
        return workflowBuilder.Build();
    }
}