using Woo.AICopilot.DataAnalysisService.Plugins;
using Woo.AICopilot.Visualization;

namespace Woo.AICopilot.DataAnalysisService.Services;

/// <summary>
/// 可视化上下文
/// 职责：在 Scoped 生命周期内暂存原始的数据库查询结果，
/// 以便后续的执行器能够获取无损数据用于构建 UI 组件。
/// </summary>
public class VisualizationContext
{
    // 存储最后一次查询的数据行（动态类型）
    private IEnumerable<dynamic>? _lastResultSet;
    
    // 存储最后一次查询的 Schema 信息（列名、类型等）
    private IEnumerable<SchemaColumn>? _lastResultSchema;

    // 存储数据分析输出结果
    private DataAnalysisOutputDto _outputDto = new ();

    /// <summary>
    /// 捕获查询结果
    /// </summary>
    public void CaptureResult(IEnumerable<dynamic> resultSet, IEnumerable<SchemaColumn> schema)
    {
        _lastResultSet = resultSet;
        _lastResultSchema = schema;
    }
    
    /// <summary>
    /// 捕获输出结果
    /// </summary>
    public void CaptureOutput(AnalysisDto? analysis, VisualDecisionDto? decision)
    {
        _outputDto.Analysis = analysis;
        _outputDto.Decision = decision;
    }

    /// <summary>
    /// 获取暂存的数据集
    /// </summary>
    public (IEnumerable<dynamic>? Data, IEnumerable<SchemaColumn>? Schema) GetLastResult()
    {
        return (_lastResultSet, _lastResultSchema);
    }
    
    /// <summary>
    /// 获取输出结果
    /// </summary>
    public DataAnalysisOutputDto GetOutput()
    {
        return _outputDto;
    }

    /// <summary>
    /// 检查是否包含有效数据
    /// </summary>
    public bool HasData => _lastResultSet != null && _lastResultSet.Any();
}

