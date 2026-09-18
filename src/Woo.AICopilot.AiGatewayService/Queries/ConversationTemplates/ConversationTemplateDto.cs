using System;

namespace Woo.AICopilot.AiGatewayService.Queries.ConversationTemplates;

public record ConversationTemplateDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string SystemPrompt { get; set; }
    public int? MaxTokens { get; set; }
    public double? Temperature { get; set; }
    public bool IsEnabled { get; set; }
}