using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Npgsql;
using Woo.AICopilot.Core.DataAnalysis.Aggregates.BusinessDatabase;
using Woo.AICopilot.Dapper.Security;
using Woo.AICopilot.Services.Common.Contracts;

namespace Woo.AICopilot.Dapper;

public class DapperDatabaseConnector(
    ISqlGuardrail sqlGuardrail,
    ILogger<DapperDatabaseConnector> logger) : IDatabaseConnector
{
    public IDbConnection GetConnection(BusinessDatabase database)
    {
        var connectionString = database.ConnectionString;

        return database.Provider switch
        {
            DbProviderType.PostgreSql => new NpgsqlConnection(connectionString),
            DbProviderType.SqlServer => new SqlConnection(connectionString),
            DbProviderType.MySql => new MySqlConnection(connectionString),
            _ => throw new NotSupportedException($"不支持的数据库提供程序: {database.Provider}")
        };
    }

    public async Task<IEnumerable<dynamic>> ExecuteQueryAsync(
        BusinessDatabase database, 
        string sql, 
        object? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        // 1. 安全检查
        var guardResult = sqlGuardrail.Validate(sql);
        if (!guardResult.IsSafe)
        {
            logger.LogWarning("SQL安全拦截: {Reason}. SQL: {Sql}", guardResult.ErrorMessage, sql);
            throw new InvalidOperationException(guardResult.ErrorMessage);
        }

        // 2. 创建连接
        using var connection = GetConnection(database);
        
        try
        {
            // 3. 执行查询
            // 使用 CommandDefinition 支持 CancellationToken
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
            
            // Dapper 的 QueryAsync 返回的是 IEnumerable<dynamic>
            // 这对于无法预知列名的动态查询非常合适
            var result = await connection.QueryAsync(command);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "在数据库 {DbName} 上执行 SQL 失败。SQL: {Sql}", database.Name, sql);
            throw; // 抛出异常供上层 Agent 捕获并进行自我修正
        }
    }
}