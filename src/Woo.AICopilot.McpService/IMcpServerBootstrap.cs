using ModelContextProtocol.Client;

namespace Woo.AICopilot.McpService;

public interface IMcpServerBootstrap
{
    IAsyncEnumerable<McpClient> StartAsync(CancellationToken cancellationToken);
}