using System;
using System.Threading;
using System.Threading.Tasks;
using Woo.AICopilot.Core.AiGateway.Aggregates.ConversationTemplate;
using Woo.AICopilot.Services.Common.Attributes;
using Woo.AICopilot.SharedKernel.Messaging;
using Woo.AICopilot.SharedKernel.Repository;
using Woo.AICopilot.SharedKernel.Result;

namespace Woo.AICopilot.AiGatewayService.Commands.ConversationTemplates;

[AuthorizeRequirement("AiGateway.DeleteConversationTemplate")]
public record DeleteConversationTemplateCommand(Guid Id) : ICommand<Result>;

public class DeleteConversationTemplateCommandHandler(IRepository<ConversationTemplate> modelRepo)
    : ICommandHandler<DeleteConversationTemplateCommand, Result>
{
    public async Task<Result> Handle(DeleteConversationTemplateCommand request, CancellationToken cancellationToken)
    {
        var model = await modelRepo.GetByIdAsync(request.Id, cancellationToken);
        if (model == null) return Result.Success();

        modelRepo.Delete(model);
        await modelRepo.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}