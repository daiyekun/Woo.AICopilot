using System.Text.Json.Serialization;

namespace Woo.AICopilot.Core.AiGateway.Aggregates.Sessions;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageType
{
    User,
    Assistant,
    System,
    Tool
}