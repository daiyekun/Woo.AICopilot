using System.Text;

namespace Woo.AICopilot.RagWorker.Services.Parsers;

public class TextDocumentParser : IDocumentParser
{
    // 支持多种纯文本格式
    public string[] SupportedExtensions => [".txt", ".md", ".json", ".xml"];

    public async Task<string> ParseAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        // 自动检测编码，默认 UTF-8
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}