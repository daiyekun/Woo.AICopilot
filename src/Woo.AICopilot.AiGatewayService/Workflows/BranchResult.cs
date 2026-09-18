using Microsoft.Extensions.AI;

namespace Woo.AICopilot.AiGatewayService.Workflows;

/// <summary>
/// 并行分支类型枚举
/// 用于标识数据来源，指导聚合器如何处理数据。
/// </summary>
public enum BranchType
{
    Tools,          // 工具定义分支
    Knowledge,      // 知识检索分支
    DataAnalysis    // 数据分析分支
}

/// <summary>
/// 分支执行结果（统一传输对象）
/// 职责：作为所有并行执行器的统一输出格式，封装异构数据。
/// </summary>
public record BranchResult
{
    /// <summary>
    /// 数据类型标识
    /// </summary>
    public BranchType Type { get; init; }

    /// <summary>
    /// 工具列表数据（当 Type == Tools 时有值）
    /// </summary>
    public AITool[]? Tools { get; init; }

    /// <summary>
    /// 知识文本数据（当 Type == Knowledge 时有值）
    /// </summary>
    public string? Knowledge { get; init; }
    
    /// <summary>
    /// 数据分析结果（当 Type == DataAnalysis 时有值）
    /// 通常包含 JSON 数据和自然语言总结
    /// </summary>
    public string? DataAnalysis { get; init; }

    // 静态工厂方法，简化创建过程
    
    public static BranchResult FromTools(AITool[] tools) => 
        new() { Type = BranchType.Tools, Tools = tools };

    public static BranchResult FromKnowledge(string knowledge) => 
        new() { Type = BranchType.Knowledge, Knowledge = knowledge };
    
    public static BranchResult FromDataAnalysis(string result) => 
        new() { Type = BranchType.DataAnalysis, DataAnalysis = result };
}