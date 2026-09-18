namespace Woo.AICopilot.SharedKernel.Domain;

public interface IAggregateRoot : IEntity;

public interface IAggregateRoot<TId> : IEntity<TId>;