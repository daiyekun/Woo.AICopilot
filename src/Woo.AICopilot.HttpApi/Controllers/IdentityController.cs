using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Woo.AICopilot.HttpApi.Infrastructure;
using Woo.AICopilot.HttpApi.Models;
using Woo.AICopilot.IdentityService.Commands;

namespace Woo.AICopilot.HttpApi.Controllers;

[Route("/api/identity")]
public class IdentityController : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterRequest request)
    {
        var result = await Sender.Send(new CreateUserCommand(request.Username, request.Password));
        
        return ReturnResult(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginRequest request)
    {
        var result = await Sender.Send(new LoginUserCommand(request.Username, request.Password));
        return ReturnResult(result);
    }

    [HttpPost("role")]
    public async Task<IActionResult> CreateRole(CreateRoleRequest request)
    {
        var result = await Sender.Send(new CreateRoleCommand(request.RoleName));
        return ReturnResult(result);
    }

    [Authorize]
    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok(new
        {
            Username = User.FindFirstValue(ClaimTypes.Name)
        });
    }
    
    [HttpGet("check")]
    public IActionResult Check()
    {
        return Ok("OK");
    }
}