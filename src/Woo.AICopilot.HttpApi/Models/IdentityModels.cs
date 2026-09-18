namespace Woo.AICopilot.HttpApi.Models;

public record UserRegisterRequest(string Username, string Password);

public record UserLoginRequest(string Username, string Password);

public record CreateRoleRequest(string RoleName);