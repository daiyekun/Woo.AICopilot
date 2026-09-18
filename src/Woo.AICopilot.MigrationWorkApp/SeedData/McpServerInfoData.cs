using Woo.AICopilot.Core.McpServer.Aggregates.McpServerInfo;

namespace Woo.AICopilot.MigrationWorkApp.SeedData;

public static class McpServerInfoData
{
    public static IEnumerable<McpServerInfo> GetMcpServerInfos()
    {
        // 1. 定义文件系统的高风险操作列表
        // 这些字符串必须与 MCP Server 提供的工具名称严格匹配
        var fileSystemRisks = new List<string> 
        { 
            "write_file", 
            "edit_file", 
            "move_file"
        };
        
        // 添加文件系统
        var fileSystemAgentPlugin = new McpServerInfo(
            "FileSystem",
            "提供本地文件系统访问能力，它允许在限定的目录范围内执行文件和目录相关操作，包括读取文件内容、创建和写入文件、列出目录结构、移动或重命名文件等。",
            McpTransportType.Stdio,
            "npx",
            @"-y @modelcontextprotocol/server-filesystem D:\Test",
            fileSystemRisks
        );  
        
        return [fileSystemAgentPlugin];
    }
}