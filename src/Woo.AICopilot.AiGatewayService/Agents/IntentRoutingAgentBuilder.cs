using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Woo.AICopilot.AgentPlugin;
using Woo.AICopilot.Services.Common.Contracts;

namespace Woo.AICopilot.AiGatewayService.Agents;
public class IntentRoutingAgentBuilder
{
    private const string AgentName = "IntentRoutingAgent";
    
    private readonly ChatAgentFactory _agentFactory;
    private readonly IDataQueryService _dataQueryService;

    // 缓存静态的工具意图列表，避免重复反射扫描
    private readonly string _toolIntentListString;
    
    public IntentRoutingAgentBuilder(
        ChatAgentFactory agentFactory, 
        AgentPluginLoader pluginLoader, 
        IDataQueryService dataQueryService)
    {
        _agentFactory = agentFactory;
        _dataQueryService = dataQueryService;

        // 预构建工具意图列表（这些是代码硬编码的，运行时不会变，可以缓存）
        var sb = new StringBuilder();
        sb.AppendLine("- General.Chat: 闲聊、打招呼、情感交互或无法归类的问题。");
        
        var allPlugins = pluginLoader.GetAllPlugin(); 
        foreach (var plugin in allPlugins)
        {
            // 格式：- Action.{PluginName}: {Description}
            sb.AppendLine($"- Action.{plugin.Name}: {plugin.Description}");
        }
        _toolIntentListString = sb.ToString();
    }
    
    /// <summary>
    /// 获取知识库意图列表
    /// </summary>
    private async Task<string> GetKnowledgeIntentListAsync()
    {
        var sb = new StringBuilder();
        
        // 查询所有启用的知识库
        var kbs = await _dataQueryService.ToListAsync(_dataQueryService.KnowledgeBases);
            
        foreach (var kb in kbs)
        {
            // 格式：- Knowledge.{KbName}: {Description}
            // 示例：- Knowledge.HrPolicy: 公司员工手册、报销制度和考勤规定。
            sb.AppendLine($"- Knowledge.{kb.Name}: {kb.Description}");
        }

        return sb.ToString();
    }
    
    /// <summary>
    /// 获取数据分析意图列表
    /// </summary>
    private async Task<string> GetDataAnalysisIntentListAsync()
    {
        var sb = new StringBuilder();

        // 查询所有启用的业务数据库
        var queryable = _dataQueryService.BusinessDatabases.Where(b => b.IsEnabled);
        var dbs = await _dataQueryService.ToListAsync(queryable);

        foreach (var db in dbs)
        {
            // 格式：- Analysis.{DbName}: {Description}
            // 示例：- Analysis.ERP_Core: 包含销售订单、客户资料及发货记录。
            sb.AppendLine($"- Analysis.{db.Name}: {db.Description}");
        }

        return sb.ToString();
    }
    
    public async Task<ChatClientAgent> BuildAsync()
    {
        var intents = new StringBuilder();
        
        // 1. 添加工具意图 (Plugin)
        intents.Append(_toolIntentListString);
        
        // 2. 添加知识库意图 (RAG)
        intents.Append(await GetKnowledgeIntentListAsync());
        
        // 3. 添加数据分析意图 (Text-to-SQL)
        intents.Append(await GetDataAnalysisIntentListAsync());
        
        var agent = await _agentFactory.CreateAgentAsync(AgentName,
            template =>
            {
                // 渲染 System Prompt
                // 确保我们在 Prompt 模板中预留了 {{$IntentList}} 占位符
                template.SystemPrompt = template.SystemPrompt
                    .Replace("{{$IntentList}}", intents.ToString());
            }, isSaveChatMessage: false);
        
        return agent;
    }
}