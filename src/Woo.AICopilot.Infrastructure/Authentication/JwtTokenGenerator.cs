using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Woo.AICopilot.IdentityService.Contracts;
using Woo.AICopilot.Services.Common.Contracts;

namespace Woo.AICopilot.Infrastructure.Authentication;

public class JwtTokenGenerator(
    IOptions<JwtSettings> jwtSettings,
    UserManager<IdentityUser> userManager) : IJwtTokenGenerator
{
    public async Task<string> GenerateTokenAsync(IdentityUser user)
    {
        // 1. 从配置中读取 JWT 设置
        var issuer = jwtSettings.Value.Issuer;
        var audience = jwtSettings.Value.Audience;
        var secretKey = jwtSettings.Value.SecretKey;
        var accessTokenExpirationMinutes = jwtSettings.Value.AccessTokenExpirationMinutes;

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        // 2. 准备 Claims (声明)
        // Claims 是 Token 中包含的用户信息，例如用户ID、用户名、角色等
        var userClaims = await userManager.GetClaimsAsync(user);
        var userRoles = await userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // 保证 Token 唯一性
        };

        // 添加用户角色到 Claims
        authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));
        // 添加自定义 Claims
        authClaims.AddRange(userClaims);

        // 3. 创建 Token 描述符
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(authClaims),
            Issuer = issuer,
            Audience = audience,
            Expires = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes),
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        };

        // 4. 创建 Token
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        // 5. 序列化为字符串
        return tokenHandler.WriteToken(token);
    }
}