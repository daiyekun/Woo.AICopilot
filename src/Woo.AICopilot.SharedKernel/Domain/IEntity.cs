namespace Woo.AICopilot.SharedKernel.Domain;

public interface IEntity;

public interface IEntity<TId> : IEntity
{
    TId Id { get; set; }
}