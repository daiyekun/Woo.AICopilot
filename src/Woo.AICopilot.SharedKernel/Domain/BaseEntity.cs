namespace Woo.AICopilot.SharedKernel.Domain;

public abstract class BaseEntity<TId> : IEntity<TId>
{
    public TId Id { get; set; } = default!;
}