using Microsoft.SemanticKernel.Text;
using Woo.AICopilot.RagWorker.Services.TokenCounter;
#pragma warning disable SKEXP0050

namespace Woo.AICopilot.RagWorker.Services;

public class TextSplitterService(ITokenCounter tokenCounter)
{
    // 默认配置：适合 Qwen3-4B 等大多数 Embedding 模型
    private const int DefaultMaxTokensPerParagraph = 500;
    private const int DefaultMaxTokensPerLine = 120;
    private const int DefaultOverlapTokens = 50;

    /// <summary>
    /// 将长文本分割为语义连贯的段落列表
    /// </summary>
    /// <param name="text">原始文本</param>
    /// <returns>切片后的文本列表</returns>
    public List<string> Split(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        // 1. 预处理：移除可能导致干扰的特殊控制字符
        var cleanText = Preprocess(text);

        // 2. 第一层切割：将文本按行（Line）拆分
        // SK 的逻辑是先按换行符等强分隔符切成小块（Lines），再将这些 Lines 组合成 Paragraphs
        // 这样可以确保尽量不在句子中间强行截断
        var lines = TextChunker.SplitPlainTextLines(
            cleanText, 
            maxTokensPerLine: DefaultMaxTokensPerLine, 
            tokenCounter: tokenCounter.CountTokens);

        // 3. 第二层组合：将 Lines 聚合成 Paragraphs
        // 这一步会严格控制 Token 数量上限，并处理重叠逻辑
        var paragraphs = TextChunker.SplitPlainTextParagraphs(
            lines, 
            maxTokensPerParagraph: DefaultMaxTokensPerParagraph, 
            overlapTokens: DefaultOverlapTokens, 
            tokenCounter: tokenCounter.CountTokens);

        return paragraphs;
    }

    private static string Preprocess(string text)
    {
        // 替换掉 Windows 的 \r\n 为 \n，统一换行符
        // 移除 NULL 字符等
        return text.Replace("\r\n", "\n").Trim();
    }
}