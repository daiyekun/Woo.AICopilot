using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Woo.AICopilot.AiGatewayService.Queries.ConversationTemplates;
using Woo.AICopilot.Core.AiGateway.Aggregates.Sessions;
using Woo.AICopilot.Services.Common.Attributes;
using Woo.AICopilot.Services.Common.Contracts;
using Woo.AICopilot.SharedKernel.Messaging;
using Woo.AICopilot.SharedKernel.Repository;
using Woo.AICopilot.SharedKernel.Result;

namespace Woo.AICopilot.AiGatewayService.Commands.Sessions;

public record CreatedSessionDto(Guid Id, string Title);

[AuthorizeRequirement("AiGateway.CreateSession")]
public record CreateSessionCommand(Guid? TemplateId) : ICommand<Result<CreatedSessionDto>>;

public class CreateSessionCommandHandler(IRepository<Session> repo, IMediator mediator, ICurrentUser user)
    : ICommandHandler<CreateSessionCommand, Result<CreatedSessionDto>>
{
    public async Task<Result<CreatedSessionDto>> Handle(CreateSessionCommand request,
        CancellationToken ct)
    {
        var tempalteId = request.TemplateId;
        
        if (tempalteId == null)
        {
            var template = await mediator.Send(new GetConversationTemplateByNameQuery("GeneralAgent"), ct);
            if (!template.IsSuccess) return Result.NotFound();
            tempalteId = template.Value!.Id;
        }
        var result = new Session(new Guid(user.Id!), tempalteId.Value);

        repo.Add(result);

        await repo.SaveChangesAsync(ct);

        return Result.Success(new CreatedSessionDto(result.Id, result.Title));
    }
}