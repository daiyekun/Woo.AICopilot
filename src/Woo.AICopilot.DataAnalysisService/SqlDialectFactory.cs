using Woo.AICopilot.Core.DataAnalysis.Aggregates.BusinessDatabase;

namespace Woo.AICopilot.DataAnalysisService;

/// <summary>
/// SQL 方言策略工厂
/// 负责提供特定数据库类型的提示词片段，用于注入 System Prompt。
/// </summary>
public static class SqlDialectFactory
{
    /// <summary>
    /// 获取指定数据库类型的方言指南
    /// </summary>
    public static string GetInstructions(DbProviderType providerType)
    {
        return providerType switch
        {
            DbProviderType.PostgreSql => PostgreSqlInstructions,
            DbProviderType.SqlServer => SqlServerInstructions,
            DbProviderType.MySql => MySqlInstructions,
            _ => "请使用标准的 ANSI SQL 语法。"
        };
    }

    // PostgreSQL 专用指令
    private const string PostgreSqlInstructions = """
                                                   - **分页**: 必须使用 `LIMIT {n} OFFSET {m}` 语法。
                                                   - **字符串连接**: 必须使用双竖线 `||` (例如 `first_name || ' ' || last_name`)。
                                                   - **当前时间**: 使用 `NOW()` 或 `CURRENT_TIMESTAMP`。
                                                   - **标识符引用**: PostgreSQL 对大小写敏感。如果表名或列名包含大写字母，**必须**使用双引号包裹 (例如 `"UserTable"`)。
                                                   - **类型转换**: 使用 `::type` 语法 (例如 `price::numeric`) 或 `CAST()`。
                                                   - **正则匹配**: 使用 `~` (区分大小写) 或 `~*` (不区分大小写)。
                                                   """;

    // SQL Server 专用指令
    private const string SqlServerInstructions = """
                                                  - **分页**: 必须使用 `TOP {n}` (例如 `SELECT TOP 10 *`) 或 `OFFSET-FETCH` 子句。
                                                  - **字符串连接**: 使用加号 `+` 或 `CONCAT()` 函数。
                                                  - **当前时间**: 使用 `GETDATE()`。
                                                  - **标识符引用**: 建议使用方括号 `[]` 包裹 (例如 `[UserTable]`)。
                                                  - **日期计算**: 使用 `DATEDIFF(day, start, end)` 或 `DATEADD(...)`。
                                                  - **注意**: 不支持 `LIMIT` 关键字。
                                                  """;

    // MySQL 专用指令
    private const string MySqlInstructions = """
                                              - **分页**: 使用 `LIMIT {n}, {m}`。
                                              - **字符串连接**: 必须使用 `CONCAT()` 函数，不支持 `||` 或 `+`。
                                              - **标识符引用**: 使用反引号 `` ` `` 包裹。
                                              - **日期**: 使用 `NOW()` 或 `CURDATE()`。
                                              """;
}