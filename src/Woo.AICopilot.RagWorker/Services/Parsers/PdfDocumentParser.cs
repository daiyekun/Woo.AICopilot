using System.Text;
using UglyToad.PdfPig;

namespace Woo.AICopilot.RagWorker.Services.Parsers;

public class PdfDocumentParser : IDocumentParser
{
    public string[] SupportedExtensions => [".pdf"];

    public Task<string> ParseAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var sb = new StringBuilder();

            try
            {
                // PdfPig 需要 Seekable 流，如果流不支持 Seek，需要复制到 MemoryStream
                using var pdfDocument = PdfDocument.Open(stream);

                foreach (var page in pdfDocument.GetPages())
                {
                    // 提取每一页的文本，并用换行符分隔
                    // 实际生产中可能需要更复杂的版面分析算法来处理多栏排版
                    var text = page.Text;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        sb.AppendLine(text);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("PDF 解析失败，文件可能已损坏或加密。", ex);
            }

            return sb.ToString();
        }, cancellationToken);
    }
}