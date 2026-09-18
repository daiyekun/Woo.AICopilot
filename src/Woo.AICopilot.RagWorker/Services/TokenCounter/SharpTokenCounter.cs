using SharpToken;

namespace Woo.AICopilot.RagWorker.Services.TokenCounter;

public class SharpTokenCounter : ITokenCounter
{
    // cl100k_base 是 GPT-3.5/4 使用的编码器，对于多语言支持较好
    private readonly GptEncoding _encoding = GptEncoding.GetEncoding("cl100k_base");

    public int CountTokens(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        
        // 获取 Token 列表的长度
        return _encoding.Encode(text).Count;
    }
}