namespace Woo.AICopilot.RagWorker.Services.Parsers;

public class DocumentParserFactory(IEnumerable<IDocumentParser> parsers)
{
    public IDocumentParser GetParser(string extension)
    {
        var ext = extension.ToLowerInvariant();
        
        // 查找支持该扩展名的解析器
        var parser = parsers.FirstOrDefault(p => p.SupportedExtensions.Any(e => e == ext));

        return parser ?? throw new NotSupportedException($"不支持的文件格式: {extension}");
    }
}