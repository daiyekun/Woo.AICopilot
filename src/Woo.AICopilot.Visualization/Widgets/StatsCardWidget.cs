namespace Woo.AICopilot.Visualization.Widgets;

public class StatsCardWidget : WidgetBase
{
    public required StatsCardData Data { get; set; }
}

public class StatsCardData
{
    public required string Label { get; set; }
    
    // 使用 object 以允许 string 或 number，但在序列化时尽量保持数值类型
    public required object Value { get; set; } 
    
    public string? Unit { get; set; }
}