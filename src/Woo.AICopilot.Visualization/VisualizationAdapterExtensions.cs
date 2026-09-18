using Woo.AICopilot.Visualization.Widgets;

namespace Woo.AICopilot.Visualization;

/// <summary>
/// 简化的 Schema 定义，用于传递给适配器
/// </summary>
public record SchemaColumn(string Name, Type DataType);

public static class VisualizationAdapterExtensions
{
    extension(IEnumerable<dynamic> rawData)
    {
        /// <summary>
        /// 将原始数据转换为通用的图表数据集
        /// </summary>
        public ChartDataset ToChartDataset(IEnumerable<SchemaColumn> schema)
        {
            var dataset = new ChartDataset();
        
            // 1. 设置维度 (Dimensions)
            dataset.Dimensions = schema.Select(s => s.Name).ToList();

            // 2. 转换数据源 (Source)
            foreach (var row in rawData)
            {
                if (row is IDictionary<string, object> rowDict)
                {
                    var cleanDict = new Dictionary<string, object?>();
                    foreach (var kvp in rowDict)
                    {
                        // 核心：处理类型兼容性
                        cleanDict[kvp.Key] = SanitizeValue(kvp.Value);
                    }
                    dataset.Source.Add(cleanDict);
                }
            }

            return dataset;
        }

        /// <summary>
        /// 将原始数据转换为数据表格结构
        /// </summary>
        public DataTableData ToDataTableData(IEnumerable<SchemaColumn> schema)
        {
            var tableData = new DataTableData();

            // 1. 构建列定义
            foreach (var col in schema)
            {
                tableData.Columns.Add(new TableColumn
                {
                    Key = col.Name,
                    Label = col.Name, // 初始 Label 等于 Key，后续可由 LLM 优化
                    DataType = MapSystemTypeToWebType(col.DataType)
                });
            }

            // 2. 构建行数据 (逻辑同上)
            foreach (var row in rawData)
            {
                if (row is IDictionary<string, object> rowDict)
                {
                    var cleanDict = new Dictionary<string, object?>();
                    foreach (var kvp in rowDict)
                    {
                        cleanDict[kvp.Key] = SanitizeValue(kvp.Value);
                    }
                    tableData.Rows.Add(cleanDict);
                }
            }

            return tableData;
        }
    }

    /// <summary>
    /// 类型清洗与标准化
    /// </summary>
    private static object? SanitizeValue(object? value)
    {
        if (value == null || value is DBNull) return null;

        return value switch
        {
            // 确保 DateTime 转换为 ISO 8601 字符串
            DateTime dt => dt.ToString("yyyy-MM-ddTHH:mm:ss"),
            DateTimeOffset dto => dto.ToString("yyyy-MM-ddTHH:mm:ss"),
            
            // 数值类型保持原样，避免被转为字符串
            decimal or double or float or int or long => value,
            
            // 其他类型转字符串
            _ => value.ToString()
        };
    }

    /// <summary>
    /// .NET 类型映射到 Web 前端类型
    /// </summary>
    private static string MapSystemTypeToWebType(Type type)
    {
        if (IsNumeric(type)) return "number";
        if (type == typeof(DateTime) || type == typeof(DateTimeOffset)) return "date";
        if (type == typeof(bool)) return "boolean";
        return "string";
    }

    private static bool IsNumeric(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type == typeof(int) || type == typeof(long) || type == typeof(double) || 
               type == typeof(decimal) || type == typeof(float);
    }

}