using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace Woo.AICopilot.McpService;

public class McpServerManager(
    IServiceScopeFactory scopeFactory,
    ILogger<McpServerManager> logger)
    : IHostedService
{
    // 用于存储所有活跃的客户端实例
    private readonly IList<McpClient> _mcpClients = [];
    
    /// <summary>
    /// 应用启动时触发
    /// </summary>
    public async Task StartAsync(CancellationToken ct)
    {
        logger.LogInformation("=== MCP Server Manager 启动中 ===");

        // 1. 显式创建作用域，以解析 Scoped 服务
        using var scope = scopeFactory.CreateScope();
        
        // 2. 从作用域中获取启动器
        var bootstrap = scope.ServiceProvider
            .GetRequiredService<IMcpServerBootstrap>();

        // 3. 消费异步流
        // 这里的 await foreach 使得只要有一个服务连接成功，就可以立即处理，
        // 而不必等待所有服务都连接完成。
        await foreach (var mcpClient in bootstrap.StartAsync(ct))
        {
            // 将客户端实例加入内存列表进行托管
            _mcpClients.Add(mcpClient);
        }

        logger.LogInformation("=== MCP Server Manager 启动完成，共托管 {Count} 个服务 ===", _mcpClients.Count);
    }

    /// <summary>
    /// 应用停止时触发
    /// </summary>
    public async Task StopAsync(CancellationToken ct)
    {
        logger.LogInformation("正在关闭 MCP 服务连接...");

        // 优雅关闭：并行释放所有客户端资源
        // 我们不希望一个客户端的关闭卡死阻碍其他客户端的关闭
        var closeTasks = _mcpClients.Select(async client => 
        {
            try
            {
                // DisposeAsync 会发送关闭信号，对于 Stdio 传输，这会 Kill 掉子进程
                await client.DisposeAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "关闭 MCP 客户端时发生错误");
            }
        });

        await Task.WhenAll(closeTasks);
        
        _mcpClients.Clear();
        logger.LogInformation("所有 MCP 服务资源已释放");
    }
}