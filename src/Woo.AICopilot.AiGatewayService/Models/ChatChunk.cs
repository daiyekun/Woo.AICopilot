using System.Text.Json.Serialization;

namespace Woo.AICopilot.AiGatewayService.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChunkType
{
    Error,
    Text,
    Intent,
    FunctionCall,
    FunctionResult,
    Widget,
    ApprovalRequest
}

public record ChatChunk(string Source, ChunkType Type, string Content);
