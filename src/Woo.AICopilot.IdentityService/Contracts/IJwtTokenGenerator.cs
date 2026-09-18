using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Woo.AICopilot.IdentityService.Contracts;

public interface IJwtTokenGenerator
{
    Task<string> GenerateTokenAsync(IdentityUser user);
}