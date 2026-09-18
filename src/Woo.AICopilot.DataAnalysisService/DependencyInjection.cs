using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Woo.AICopilot.AgentPlugin;
using Woo.AICopilot.Dapper;
using Woo.AICopilot.DataAnalysisService.Services;
using Woo.AICopilot.Visualization;

namespace Woo.AICopilot.DataAnalysisService;

public static class DependencyInjection
{
    public static void AddDataAnalysisService(this IHostApplicationBuilder builder)
    {
        builder.AddDapper();
        builder.Services.AddScoped<VisualizationContext>();
        // 注册插件加载器
        builder.Services.AddAgentPlugin(registrar =>
        {
            registrar.RegisterPluginFromAssembly(Assembly.GetExecutingAssembly());
        });
    }
}