namespace Woo.AICopilot.Visualization.Widgets;

public class DataTableWidget : WidgetBase
{
    public required DataTableData Data { get; set; }
}

public class DataTableData
{
    public List<TableColumn> Columns { get; set; } = new();
    
    // 使用 Dictionary<string, object> 来表示动态行数据
    public List<Dictionary<string, object?>> Rows { get; set; } = new();
}

public class TableColumn
{
    public required string Key { get; set; }
    public required string Label { get; set; }
    public string DataType { get; set; } = "string"; // string, number, date, boolean
}