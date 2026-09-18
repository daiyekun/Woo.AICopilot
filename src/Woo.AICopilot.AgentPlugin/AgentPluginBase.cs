using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.AI;
#pragma warning disable MEAI001

namespace Woo.AICopilot.AgentPlugin;

public abstract class AgentPluginBase : IAgentPlugin
{
    // 默认实现：直接使用类名作为插件名称
    public virtual string Name { get; } 
    
    public virtual string Description { get; protected set; } = string.Empty;

    protected AgentPluginBase()
    {
        Name = GetType().Name;
    }

    /// <summary>
    /// 核心逻辑：扫描当前类中所有标记了 [Description] 的公共方法。
    /// 只有带有描述的方法才会被视为 AI 工具。
    /// </summary>
    private IEnumerable<MethodInfo> GetToolMethods()
    {
        var type = GetType();
        return type.GetMethods(BindingFlags.Instance | BindingFlags.Public)      
            .Where(m => m.GetCustomAttribute<DescriptionAttribute>() != null);
    }
    
    /// <summary>
    /// 利用 Microsoft.Extensions.AI 库，将 C# 方法自动转换为 AITool。
    /// 并根据 HighRiskTools 配置，自动为敏感工具添加审批拦截器。
    /// </summary>
    public IEnumerable<AITool>? GetAITools()
    {
        // 1. 获取所有标记了 [Description] 的方法
        // 这些是原始的业务逻辑方法
        var rawMethods = GetToolMethods();

        // 2. 转换为 AITool 并注入拦截逻辑
        var tools = rawMethods.Select(method =>
        {
            // 步骤 A: 创建基础 AI 函数
            // AIFunctionFactory.Create 是微软提供的工厂方法，
            // 它会读取方法签名、参数类型和 Description 特性，生成 JSON Schema。
            // 'this' 参数确保了当工具被调用时，是在当前插件实例上执行的。
            var function = AIFunctionFactory.Create(method, this);

            // 步骤 B: 检查该方法是否在高风险列表中
            if (HighRiskTools == null || !HighRiskTools.Contains(method.Name)) 
            {
                // 如果是普通工具，直接返回
                return function;
            }

            // 步骤 C: 注入审批拦截器
            // 如果是高风险工具，我们不直接返回原始 function，
            // 而是将其包装在 ApprovalRequiredAIFunction 中。
            // 当 LLM 尝试调用此工具时，框架会识别这个包装器，并挂起执行，等待人工审批。
            var approvalFunction = new ApprovalRequiredAIFunction(function);
            
            return approvalFunction;
        });

        return tools;
    }

    public virtual IEnumerable<string>? HighRiskTools { get; init; }
}