using System.Text.Json.Serialization;

namespace Woo.AICopilot.Visualization.Widgets;

/// <summary>
/// 可视化组件类型的枚举定义
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WidgetType
{
    StatsCard,  // 统计卡片
    DataTable,  // 数据表格
    Chart      // 通用图表
}

/// <summary>
/// 可视化组件通用契约
/// </summary>
public interface IWidget
{
    /// <summary>
    /// 组件标题
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// 组件描述信息
    /// </summary>
    string? Description { get; set; }
    
}

/// <summary>
/// 可视化组件基类
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(StatsCardWidget), typeDiscriminator: nameof(WidgetType.StatsCard))]
[JsonDerivedType(typeof(DataTableWidget), typeDiscriminator: nameof(WidgetType.DataTable))]
[JsonDerivedType(typeof(ChartWidget), typeDiscriminator: nameof(WidgetType.Chart))]
public abstract class WidgetBase : IWidget
{
    /// <summary>
    /// 组件标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 组件描述信息
    /// </summary>
    public string? Description { get; set; }

}