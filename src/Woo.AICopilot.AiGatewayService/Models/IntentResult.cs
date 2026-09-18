using System.Text.Json.Serialization;

namespace Woo.AICopilot.AiGatewayService.Models;

/// <summary>
/// 意图识别的标准输出结果
/// </summary>
public record IntentResult
{
    /// <summary>
    /// 意图标识符
    /// 规范：
    /// - 工具类：Action.{PluginName}
    /// - 知识类：Knowledge.{KnowledgeBaseName}
    /// </summary>
    [JsonPropertyName("intent")]
    public string Intent { get; set; } = string.Empty;

    /// <summary>
    /// 置信度 (0.0 - 1.0)
    /// 用于下游节点的“置信度门控”机制
    /// </summary>
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    /// <summary>
    /// 推理过程
    /// 强制模型输出思维链，提高分类准确度
    /// </summary>
    [JsonPropertyName("reasoning")]
    public string? Reasoning { get; set; }
    
    /// <summary>
    /// 检索参数
    /// 如果是 Knowledge 意图，模型可以在此提取查询关键词
    /// </summary>
    [JsonPropertyName("query")]
    public string? Query { get; set; }
}