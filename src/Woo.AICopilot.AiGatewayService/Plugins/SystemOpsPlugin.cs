using System;
using System.Collections.Generic;

namespace Woo.AICopilot.AiGatewayService.Plugins;

using System.ComponentModel;
using Woo.AICopilot.AgentPlugin;

public class SystemOpsPlugin : AgentPluginBase
{
    public override string Description => "提供系统级别的运维操作能力，如时间查询、服务重启等。";
    
    // 在此处静态定义：RestartServer 是高风险工具
    // 使用 nameof 关键字可以避免硬编码字符串带来的拼写错误风险，并支持重构
    public override IEnumerable<string> HighRiskTools => [nameof(RestartServer)];
    
    [Description("获取当前系统时间")]
    public string GetSystemTime() => DateTime.Now.ToString("O");

    [Description("执行服务器重启操作")]
    public string RestartServer()
    {
        // 实际场景中，这里可能会调用 Process.Start("shutdown", "/r /t 0");
        return "Server restart command issued successfully.";
    }
}