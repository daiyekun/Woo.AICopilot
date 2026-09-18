namespace Woo.AICopilot.Core.AiGateway.Aggregates.ConversationTemplate;

public record TemplateSpecification
{
    public int? MaxTokens { get; set; }
    public float? Temperature { get; set; }
}