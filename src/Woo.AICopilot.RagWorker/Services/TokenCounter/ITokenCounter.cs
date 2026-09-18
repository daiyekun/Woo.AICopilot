namespace Woo.AICopilot.RagWorker.Services.TokenCounter;

public interface ITokenCounter
{
    /// <summary>
    /// 计算输入文本的 Token 数量
    /// </summary>
    int CountTokens(string text);
}