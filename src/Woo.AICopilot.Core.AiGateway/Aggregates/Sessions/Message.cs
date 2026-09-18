using Woo.AICopilot.SharedKernel.Domain;

namespace Woo.AICopilot.Core.AiGateway.Aggregates.Sessions;

public class Message : IEntity<int>
{
    protected Message()
    {
    }

    public Message(Session session, string content, MessageType type)
    {
        Session = session;
        SessionId = session.Id;
        Content = content;
        Type = type;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid SessionId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public MessageType Type { get; set; }

    public Session Session { get; set; } = null!;
    public int Id { get; set; }
}