namespace Woo.AICopilot.Dapper.Security;

public interface ISqlGuardrail
{
    /// <summary>
    /// 验证SQL语句是否安全
    /// </summary>
    /// <param name="sql">待执行的SQL</param>
    /// <returns>验证结果，包含是否通过及错误信息</returns>
    (bool IsSafe, string? ErrorMessage) Validate(string sql);
}