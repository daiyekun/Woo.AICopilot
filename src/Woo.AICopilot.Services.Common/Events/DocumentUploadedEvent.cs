using System;

namespace Woo.AICopilot.Services.Common.Events;

public record DocumentUploadedEvent
{
    public Guid KnowledgeBaseId { get; init; }
    public int DocumentId { get; init; }
    public string FilePath { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
}