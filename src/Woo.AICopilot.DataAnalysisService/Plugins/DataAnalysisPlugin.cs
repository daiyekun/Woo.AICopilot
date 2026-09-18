using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Woo.AICopilot.AgentPlugin;
using Woo.AICopilot.Core.DataAnalysis.Aggregates.BusinessDatabase;
using Woo.AICopilot.DataAnalysisService.Services;
using Woo.AICopilot.Services.Common.Contracts;
using Woo.AICopilot.Services.Common.Helper;
using Woo.AICopilot.Visualization;

namespace Woo.AICopilot.DataAnalysisService.Plugins;

// 用于映射元数据查询结果
public record ColumnMetadata
{
    public string ColumnName { get; set; } = "";
    public string DataType { get; set; } = "";
    public bool IsPrimaryKey { get; set; }
    public string? Description { get; set; }
}

public class DataAnalysisPlugin(
    IDatabaseConnector dbConnector,
    ILogger<DataAnalysisPlugin> logger) : AgentPluginBase
{
    public override string Description => "提供数据库结构查询和SQL执行能力，用于回答涉及业务数据的统计分析问题。";
    
    // 辅助方法：根据名称获取数据库配置
    // 这个方法不暴露给 AI，仅供内部使用
    private async Task<BusinessDatabase> GetDatabaseAsync(IServiceProvider sp, string databaseName, CancellationToken ct)
    {
        var dataQuery = sp.GetRequiredService<IDataQueryService>();
        var queryable = dataQuery.BusinessDatabases.Where(d => d.Name == databaseName);
        var db = await dataQuery.FirstOrDefaultAsync(queryable);

        if (db == null)
        {
            throw new ArgumentException($"未找到名称为 '{databaseName}' 的数据库。请检查名称是否正确。");
        }

        if (!db.IsEnabled)
        {
            throw new InvalidOperationException($"数据库 '{databaseName}' 已被禁用。");
        }

        return db;
    }

    [Description("获取指定数据库中所有表的名称和描述。这是探索数据库结构的第一步。")]
    public async Task<string> GetTableNamesAsync(
        IServiceProvider sp,
        [Description("目标数据库的名称")] string databaseName)
    {
        try
        {
            // 获取数据库配置
            var db = await GetDatabaseAsync(sp, databaseName, CancellationToken.None);

            // 根据数据库类型构建查询元数据的 SQL
            var sql = string.Empty;
            switch (db.Provider)
            {
                case DbProviderType.PostgreSql:
                    // PostgreSQL: 从 information_schema 获取表名，关联 pg_description 获取注释
                    sql = @"
                            SELECT 
                                t.table_name AS ""TableName"",
                                obj_description(pgc.oid) AS ""Description""
                            FROM information_schema.tables t
                            INNER JOIN pg_class pgc ON t.table_name = pgc.relname
                            WHERE t.table_schema = 'public' 
                              AND t.table_type = 'BASE TABLE';";
                    break;
                case DbProviderType.SqlServer:
                    // SQL Server
                    break;
                default:
                    return $"错误：不支持的数据库类型 {db.Provider}";
            }

            // 执行查询
            // 这里使用了基础设施层的 ExecuteQueryAsync，它返回 IEnumerable<dynamic>
            var result = await dbConnector.ExecuteQueryAsync(db, sql);

            // 序列化结果
            return result.ToJson();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取表名失败。Database: {DbName}", databaseName);
            return $"获取表名时发生错误: {ex.Message}";
        }
    }

    [Description("获取指定表的详细结构定义(DDL)，包含列名、数据类型、主键和外键信息。")]
    public async Task<string> GetTableSchemaAsync(
        IServiceProvider sp,
        [Description("目标数据库的名称")] string databaseName,
        [Description("需要查询的表名列表，如 'Orders, Customers'")] string[] tableNames)
    {
        if (tableNames.Length == 0)
        {
            return "错误：请提供至少一个表名。";
        }

        try
        {
            var db = await GetDatabaseAsync(sp, databaseName, CancellationToken.None);
            var ddlBuilder = new StringBuilder();

            foreach (var tableName in tableNames)
            {
                // 1. 查询列信息
                var columns = await GetColumnsAsync(db, tableName);

                if (!columns.Any())
                {
                    ddlBuilder.AppendLine($"-- 警告: 表 '{tableName}' 不存在或没有列。");
                    continue;
                }

                // 2. 构建 CREATE TABLE 语句
                ddlBuilder.AppendLine($"CREATE TABLE {tableName} (");

                var columnDefs = new List<string>();
                foreach (var col in columns)
                {
                    // 格式: ColumnName DataType [PK/FK] [Comment]
                    var colDef = $"  {col.ColumnName} {col.DataType}";

                    if (col.IsPrimaryKey) colDef += " PRIMARY KEY";

                    // 如果有描述，作为注释添加，帮助 AI 理解字段含义
                    if (!string.IsNullOrWhiteSpace(col.Description))
                    {
                        colDef += $" -- {col.Description}";
                    }

                    columnDefs.Add(colDef);
                }

                ddlBuilder.AppendLine(string.Join(",\n", columnDefs));
                ddlBuilder.AppendLine(");");
                ddlBuilder.AppendLine();
            }

            return ddlBuilder.ToString();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取表结构失败。Database: {DbName}", databaseName);
            return $"获取表结构时发生错误: {ex.Message}";
        }
    }

    // 内部辅助方法：查询单个表的列元数据
    private async Task<List<ColumnMetadata>> GetColumnsAsync(BusinessDatabase db, string tableName)
    {
        var sql = string.Empty;
        switch (db.Provider)
        {
            case DbProviderType.PostgreSql:
                // PostgreSQL 元数据查询
                // 包含列名、类型、是否主键
                // 注意：此处简化了查询，实际生产中可能需要更复杂的关联来获取外键
                sql = @"
                SELECT 
                    c.column_name AS ""ColumnName"",
                    c.data_type AS ""DataType"",
                    CASE WHEN tc.constraint_type = 'PRIMARY KEY' THEN 1 ELSE 0 END AS ""IsPrimaryKey"",
                    pg_catalog.col_description(format('%s.%s', c.table_schema, c.table_name)::regclass::oid, c.ordinal_position) AS ""Description""
                FROM information_schema.columns c
                LEFT JOIN information_schema.key_column_usage kcu 
                    ON c.table_name = kcu.table_name AND c.column_name = kcu.column_name
                LEFT JOIN information_schema.table_constraints tc 
                    ON kcu.constraint_name = tc.constraint_name AND tc.constraint_type = 'PRIMARY KEY'
                WHERE c.table_name = @TableName AND c.table_schema = 'public';";
                break;
            case DbProviderType.SqlServer:
                // SQL Server 元数据查询
                break;
            default:
                return [];
        }
        
        var result = await dbConnector.ExecuteQueryAsync(db, sql, new { TableName = tableName });

        // Dapper 返回的是 dynamic，需要手动映射到强类型
        var columns = new List<ColumnMetadata>();
        foreach (var row in result)
        {
            var dict = (IDictionary<string, object>)row;
            columns.Add(new ColumnMetadata
            {
                ColumnName = dict["ColumnName"] as string ?? "",
                DataType = dict["DataType"] as string ?? "",
                IsPrimaryKey = Convert.ToInt32(dict["IsPrimaryKey"]) == 1,
                Description = dict["Description"] as string ?? ""
            });
        }

        return columns;
    }

    [Description("在指定数据库上执行查询 SQL 语句，并返回 JSON 格式的结果。")]
    public async Task<string> ExecuteSqlQueryAsync(
        IServiceProvider sp,
        [Description("目标数据库的名称")] string databaseName,
        [Description("要执行的 SQL 查询语句 (仅限 SELECT，不需要人类可读，去除换行符)")] string sqlQuery)
    {
        // 1. 基础校验
        if (string.IsNullOrWhiteSpace(sqlQuery)) return "错误：SQL 语句不能为空。";

        try
        {
            var db = await GetDatabaseAsync(sp, databaseName, CancellationToken.None);

            // 2. 执行查询
            var result = await dbConnector.ExecuteQueryAsync(db, sqlQuery);
            var data = result.ToList();
            var firstRow = data.FirstOrDefault() as IDictionary<string, object>;
            var schema = new List<SchemaColumn>();
        
            if (firstRow != null)
            {
                foreach (var kvp in firstRow)
                {
                    // 获取值的运行时类型，如果为 null 则默认为 object
                    var type = kvp.Value?.GetType() ?? typeof(object);
                    schema.Add(new SchemaColumn(kvp.Key, type));
                }
            }
            // 3.【关键步骤】将原始结果捕获到上下文中
            var vizContext = sp.GetRequiredService<VisualizationContext>();
            vizContext.CaptureResult(data, schema);
            
            return data.Count == 0 ?
                // 没有数据
                "查询执行成功，但未返回任何结果 (0 rows)。" :
                // LLM 只需要看摘要，不需要看完整的数据，仅取前 5 行
                data.Take(5).ToJson();
        }
        catch (InvalidOperationException ex) // 捕获安全拦截异常
        {
            logger.LogWarning("SQL 执行被拦截: {Message}", ex.Message);
            return $"安全警告: 查询被系统拒绝。原因: {ex.Message}";
        }
        catch (Exception ex)
        {
            // 这里是 ReAct 模式中“错误自愈”的关键！
            // 我们必须返回详细的数据库错误信息（如 "Column 'xxx' not found"），
            // 这样 Agent 才能看到错误 -> 思考原因 -> 修正 SQL -> 重试。
            logger.LogError(ex, "SQL 执行异常");
            return $"SQL 执行错误: {ex.Message}\n请检查你的 SQL 语法、表名或列名是否正确，并参考之前的 Schema 定义进行修正。";
        }
    }

    [Description("输出数据分析结果和可视化决策结果")]
    public string OutputResult(IServiceProvider sp,
        [Description("数据分析结果，如果查询数据失败，此字段可以 null")]
        AnalysisDto? analysis,
        [Description("可视化决策结果，参考【可视化输出规范决策指南】，如果数据不适合可视化，此字段可为 null")]
        VisualDecisionDto? decision)
    {
        // 将输出结果捕获到上下文中
        var vizContext = sp.GetRequiredService<VisualizationContext>();
        vizContext.CaptureOutput(analysis, decision);
        return "已成功获取输出结果，请直接回复‘数据查询分析任务完成，结果如下：’";
    }
}