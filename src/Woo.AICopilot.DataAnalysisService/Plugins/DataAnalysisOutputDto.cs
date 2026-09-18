using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Woo.AICopilot.Visualization.Widgets;

namespace Woo.AICopilot.DataAnalysisService.Plugins;

public record DataAnalysisOutputDto
{
    // 对应 Agent 返回的 "Analysis" 数组
    // 这里使用 object 或 JsonElement 都可以，因为我们不需要修改它，只需透传
    [JsonPropertyName("analysis")]
    public AnalysisDto? Analysis { get; set; }

    // 对应 Agent 返回的 "visual_decision" 对象
    [JsonPropertyName("visual_decision")]
    public VisualDecisionDto? Decision { get; set; }
}

public record AnalysisDto
{
    [Description("当前数据库名称")]
    [JsonPropertyName("database_name")]
    public string DatabaseName { get; set; } = string.Empty;
    
    [Description("据用户问题生成的简短数据说明")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [Description("查询结果中每个字段的定义")]
    [JsonPropertyName("metadata")]
    public List<MetadataItemDto> Metadata { get; set; } = [];
}

public record MetadataItemDto
{
    [Description("字段名")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [Description("从表结构中获取的字段注释/说明")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public record VisualDecisionDto
{
    [Description("图表类型，可选值: Chart, DataTable, StatsCard")]   
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WidgetType Type { get; set; }

    [Description("图表标题")]
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [Description("根据用户问题生成的简短数据说明")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [Description("可选字段，Chart 类型图表专有字段")]
    [JsonPropertyName("chart_config")]
    public ChartConfig? ChartConfig { get; set; }
    
    [Description("可选字段，StatsCard 类型图表专有字段")]
    [JsonPropertyName("unit")]
    public string? Unit { get; set; }
}

public record ChartConfig
{
    [Description("图表类型，可选值: Line,Bar,Pie")]
    [JsonPropertyName("category")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChartCategory Category { get; set; }

    [Description("作为X轴的字段名，例如 order_date")]
    [JsonPropertyName("x")]
    public string X { get; set; } = string.Empty;
    
    [Description("作为Y轴的数值字段名，例如 total_amount")]
    [JsonPropertyName("y")]
    public string Y { get; set; } = string.Empty;
    
    [Description("可选，用于分组的字段名，例如 product_category")]
    [JsonPropertyName("series")]
    public string? Series { get; set; }
}
