using System.Text.Json.Serialization;

namespace Woo.AICopilot.Visualization.Widgets;

public class ChartWidget : WidgetBase
{
    public required ChartData Data { get; set; }
}

public class ChartData
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChartCategory Category { get; set; }

    public ChartDataset Dataset { get; set; } = new();

    public ChartEncoding? Encoding { get; set; }
}

public enum ChartCategory
{
    Line,
    Bar,
    Pie
}

public class ChartDataset
{
    /// <summary>
    /// 维度定义（即表头）
    /// </summary>
    public List<string> Dimensions { get; set; } = [];

    /// <summary>
    /// 数据源，每一项对应一行数据
    /// key 必须与 Dimensions 中的名称匹配
    /// </summary>
    public List<Dictionary<string, object?>> Source { get; set; } = [];
}

public class ChartEncoding
{
    /// <summary>
    /// X 轴映射的维度 Key
    /// </summary>
    public string? X { get; set; }

    /// <summary>
    /// Y 轴映射的维度 Key 列表（支持多系列）
    /// </summary>
    public List<string> Y { get; set; } = [];
    
    /// <summary>
    /// 系列分组字段（可选）
    /// 例如：按 "Region" 字段分组，生成多条折线
    /// </summary>
    public string? SeriesName { get; set; }
}