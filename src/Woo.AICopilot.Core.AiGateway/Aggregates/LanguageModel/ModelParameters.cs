namespace Woo.AICopilot.Core.AiGateway.Aggregates.LanguageModel;

public record ModelParameters
{
    public int MaxTokens { get; set; }
    public float Temperature { get; set; } = 0.7f;
}