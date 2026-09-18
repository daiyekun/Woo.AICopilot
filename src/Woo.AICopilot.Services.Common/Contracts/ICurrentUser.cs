namespace Woo.AICopilot.Services.Common.Contracts;

public interface ICurrentUser
{
    string? Id { get; }

    string? UserName { get; }

    string? Role { get; }

    bool IsAuthenticated { get; }
}