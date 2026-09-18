namespace Woo.AICopilot.Core.DataAnalysis.Aggregates.BusinessDatabase;

/// <summary>
/// 数据库提供程序类型
/// </summary>
public enum DbProviderType
{
    /// <summary>
    /// PostgreSQL 数据库
    /// </summary>
    PostgreSql = 1,

    /// <summary>
    /// Microsoft SQL Server
    /// </summary>
    SqlServer = 2,

    /// <summary>
    /// MySQL (预留)
    /// </summary>
    MySql = 3
}