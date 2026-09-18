using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Woo.AICopilot.McpService;

public static class DependencyInjection
{
    public static void AddMcpService(this IHostApplicationBuilder builder)
    {
        // 注册启动器为 Scoped，因为它依赖于 Scoped 的 DbContext
        builder.Services.AddScoped<IMcpServerBootstrap, McpServerBootstrap>();
        
        // 注册管理器为 HostedService
        // 这是一个 Singleton 单例，会在应用生命周期内一直存在
        builder.Services.AddHostedService<McpServerManager>();
    }
}