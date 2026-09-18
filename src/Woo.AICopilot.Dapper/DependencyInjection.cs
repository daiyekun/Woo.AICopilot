using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Woo.AICopilot.Dapper.Security;
using Woo.AICopilot.Services.Common.Contracts;

namespace Woo.AICopilot.Dapper;

public static class DependencyInjection
{
    public static void AddDapper(this IHostApplicationBuilder builder)
    {
        // 注册 SQL 安全服务
        builder.Services.AddSingleton<ISqlGuardrail, KeywordSqlGuardrail>();
        
        // 注册 数据库连接器
        builder.Services.AddSingleton<IDatabaseConnector, DapperDatabaseConnector>();
    }
}