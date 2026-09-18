using System.Text.RegularExpressions;

namespace Woo.AICopilot.Dapper.Security;

/// <summary>
/// 基于关键词黑名单的SQL安全服务
/// 注意：这只是第一道防线，不能完全替代数据库层面的权限控制。
/// </summary>
public class KeywordSqlGuardrail : ISqlGuardrail
{
    // 定义高危操作关键词
    private static readonly string[] ForbiddenKeywords = 
    [
        "DROP", "TRUNCATE", "DELETE", "UPDATE", "INSERT", 
        "ALTER", "GRANT", "REVOKE", "CREATE", "EXEC", "EXECUTE",
        "MERGE", "REPLACE", "UPSERT"
    ];

    public (bool IsSafe, string? ErrorMessage) Validate(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return (false, "SQL语句为空");

        // 移除注释，防止通过注释绕过检测 (简单的 -- 或 /* */)
        // 生产环境建议使用更完善的 SQL Parser 库进行 AST 分析
        var cleanSql = RemoveComments(sql).ToUpperInvariant();

        foreach (var keyword in ForbiddenKeywords)
        {
            // 使用单词边界匹配，避免误杀 (例如: "SELECT * FROM UPDATE_LOG" 不应被拦截)
            var regex = new Regex($@"\b{keyword}\b", RegexOptions.IgnoreCase);
            // \b{keyword}\b 确保匹配的是完整的单词
            if (regex.IsMatch(cleanSql))
            {
                return (false, $"安全拦截：检测到禁止的关键字 '{keyword}'。Agent 仅允许执行查询操作。");
            }
        }

        // 检查是否包含分号，防止多语句执行注入 (如: SELECT * FROM Users; DROP TABLE Logs)
        // 大多数 ORM 在单次执行中只允许一条语句，但进行显式检查更为安全
        if (cleanSql.Count(c => c == ';') > 1 || (cleanSql.Contains(';') && !cleanSql.TrimEnd().EndsWith(';')))
        {
            return (false, "安全拦截：禁止在单次调用中执行多条 SQL 语句。");
        }

        return (true, null);
    }

    private static string RemoveComments(string sql)
    {
        // 移除 -- 单行注释
        var noSingleLine = Regex.Replace(sql, "--.*", "");
        // 移除 /* */ 多行注释
        var noComments = Regex.Replace(noSingleLine, @"/\*[\s\S]*?\*/", "");
        return noComments;
    }
}