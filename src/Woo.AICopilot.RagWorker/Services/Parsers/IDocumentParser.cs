namespace Woo.AICopilot.RagWorker.Services.Parsers;

public interface IDocumentParser
{
    /// <summary>
    /// 支持的文件扩展名 (如 ".pdf")
    /// </summary>
    string[] SupportedExtensions { get; }

    /// <summary>
    /// 解析文件流为纯文本
    /// </summary>
    /// <param name="stream">文件流</param>
    /// <returns>提取出的文本内容</returns>
    Task<string> ParseAsync(Stream stream, CancellationToken cancellationToken = default);
}