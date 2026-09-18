using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Woo.AICopilot.AiGatewayService.Workflows.Executors;

namespace Woo.AICopilot.AiGatewayService.Workflows;

public static class IntentWorkflow
{
    public static void AddIntentWorkflow(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IntentRoutingExecutor>();
        builder.Services.AddScoped<ToolsPackExecutor>();
        builder.Services.AddScoped<KnowledgeRetrievalExecutor>();
        builder.Services.AddScoped<DataAnalysisExecutor>();
        builder.Services.AddScoped<ContextAggregatorExecutor>();
        builder.Services.AddScoped<FinalAgentBuildExecutor>();
        builder.Services.AddScoped<FinalAgentRunExecutor>();
        builder.Services.AddScoped<WorkflowFactory>();
    }
}