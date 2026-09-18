using Woo.AICopilot.SharedKernel.Domain;

namespace Woo.AICopilot.Core.McpServer.Aggregates.McpServerInfo
{
    /// <summary>
    /// 定义 MCP 服务的配置信息，作为聚合根存在
    /// </summary>
    public class McpServerInfo : IAggregateRoot
    {
        protected McpServerInfo()
        {
        }

        public McpServerInfo(
            string name, 
            string description, 
            McpTransportType transportType, 
            string? command, 
            string arguments,
            List<string>? sensitiveTools = null)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            TransportType = transportType;
            Command = command;
            Arguments = arguments;
            IsEnabled = true;
            SensitiveTools = sensitiveTools;
        }
        
        public Guid Id { get; set; }
        
        // 服务名称，作为插件的唯一标识，例如 "github-server"
        public string Name { get; private set; }
        
        // 服务描述
        public string Description { get; private set; }
        
        // 传输类型：Stdio 或 Sse
        public McpTransportType TransportType { get; private set; }
        
        // 针对 Stdio 的配置：可执行文件路径 (如 "node", "python")
        public string? Command { get; private set; }
        
        // 针对 Stdio 的配置：启动参数 (如 "build/index.js")
        // 针对 SSE 的配置：目标 URL
        public string Arguments { get; private set; }
        
        // 是否启用
        public bool IsEnabled { get; private set; }
        
        public List<string>? SensitiveTools { get; private set; }
    }
}