using Woo.AICopilot.SharedKernel.Domain;

namespace Woo.AICopilot.Core.AiGateway.Aggregates.ConversationTemplate;

public class ConversationTemplate : IAggregateRoot
{
    protected ConversationTemplate()
    {
    }

    public ConversationTemplate(
        string name,
        string description,
        string systemPrompt,
        Guid modelId,
        TemplateSpecification specification)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        SystemPrompt = systemPrompt;
        Specification = specification;
        ModelId = modelId;
        IsEnabled = true;
    }

    public Guid Id { get; set; }
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string SystemPrompt { get; set; } = null!;

    public Guid ModelId { get; set; }

    public TemplateSpecification Specification { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public void UpdateSpecification(TemplateSpecification spec)
    {
        Specification = spec;
    }
}