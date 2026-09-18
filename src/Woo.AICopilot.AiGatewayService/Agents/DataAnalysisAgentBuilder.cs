using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Woo.AICopilot.AgentPlugin;
using Woo.AICopilot.DataAnalysisService;
using Woo.AICopilot.DataAnalysisService.Plugins;

namespace Woo.AICopilot.AiGatewayService.Agents;

public class DataAnalysisAgentBuilder(ChatAgentFactory agentFactory, AgentPluginLoader pluginLoader)
{
    private const string TemplateName = "DataAnalysisAgent";

    /// <summary>
    /// 构建针对特定数据库优化的 DBA Agent
    /// </summary>
    /// <param name="database">目标数据库实体，用于决策方言策略</param>
    /// <returns>配置好的 ChatClientAgent 实例</returns>
    public async Task<ChatClientAgent> BuildAsync(BusinessDatabase database)
    {
        // 1. 获取方言策略
        // 根据数据库类型 (PG/SQLServer)，获取对应的提示片段
        var dialectInstructions = SqlDialectFactory.GetInstructions(database.Provider);
        var providerName = database.Provider.ToString();

        // 2. 创建 Agent 并执行模版替换
        // ChatAgentFactory 会加载基础模版，我们通过回调函数进行模版替换
        var agent = await agentFactory.CreateAgentAsync(TemplateName, template =>
        {
            // 我们只替换与"数据库类型"相关的占位符。
            // 此时 Agent 尚不知道具体的查询任务是什么。
            template.SystemPrompt = template.SystemPrompt
                .Replace("{{$DbProvider}}", providerName)
                .Replace("{{$DatabaseName}}", database.Name)
                .Replace("{{$DialectInstructions}}", dialectInstructions);
        }, options =>
        {
            // 挂载 "DataAnalysisPlugin" 中的所有工具
            // 这样 Agent 就拥有了 GetTableNames, ExecuteSqlQuery 
            options.Tools = pluginLoader.GetAITools(nameof(DataAnalysisPlugin));
        }, false);
        
        return agent;
    }
}