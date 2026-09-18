namespace Woo.AICopilot.Infrastructure.Authentication;

public record JwtSettings
{
    public required string Issuer { get; set; }

    public required string Audience { get; set; }

    public required string SecretKey { get; set; }

    public int AccessTokenExpirationMinutes { get; set; } = 30;
}