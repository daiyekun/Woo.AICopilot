using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Woo.AICopilot.Services.Common.Attributes;
using Woo.AICopilot.SharedKernel.Messaging;
using Woo.AICopilot.SharedKernel.Result;

namespace Woo.AICopilot.IdentityService.Commands;

public record CreatedRoleDto(string Id, string RoleName);

[AuthorizeRequirement("Identity.CreateRole")]
public record CreateRoleCommand(string RoleName) : ICommand<Result<CreatedRoleDto>>;

public class CreateRoleCommandHandler(
    RoleManager<IdentityRole> roleManager) : ICommandHandler<CreateRoleCommand, Result<CreatedRoleDto>>
{
    public async Task<Result<CreatedRoleDto>> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var role = new IdentityRole
        {
            Name = command.RoleName
        };

        var result = await roleManager.CreateAsync(role);

        return !result.Succeeded
            ? Result.Failure(result.Errors)
            : Result.Success(new CreatedRoleDto(role.Id, role.Name));
    }
}