using Woo.AICopilot.SharedKernel.Domain;

namespace Woo.AICopilot.Core.AiGateway.Aggregates.Sessions;

public class Session : IAggregateRoot
{
    private readonly List<Message> _messages = [];

    protected Session()
    {
    }

    public Session(Guid userId, Guid templateId)
    {
        Id = Guid.NewGuid();
        Title = $"新会话[{DateTime.Now:MMddHHmm}]";
        UserId = userId;
        TemplateId = templateId;
    }

    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public Guid UserId { get; set; }
    public Guid TemplateId { get; set; }

    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

    public void AddMessage(string content, MessageType type)
    {
        var message = new Message(this, content, type);
        _messages.Add(message);
    }
}