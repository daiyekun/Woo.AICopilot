using System;
using System.Threading;
using System.Threading.Tasks;
using Woo.AICopilot.Core.AiGateway.Aggregates.LanguageModel;
using Woo.AICopilot.Services.Common.Attributes;
using Woo.AICopilot.SharedKernel.Messaging;
using Woo.AICopilot.SharedKernel.Repository;
using Woo.AICopilot.SharedKernel.Result;

namespace Woo.AICopilot.AiGatewayService.Commands.LanguageModels;

[AuthorizeRequirement("AiGateway.DeleteLanguageModel")]
public record DeleteLanguageModelCommand(Guid Id) : ICommand<Result>;

public class DeleteLanguageModelCommandHandler(IRepository<LanguageModel> repo)
    : ICommandHandler<DeleteLanguageModelCommand, Result>
{
    public async Task<Result> Handle(DeleteLanguageModelCommand request, CancellationToken cancellationToken)
    {
        var result = await repo.GetByIdAsync(request.Id, cancellationToken);
        if (result == null) return Result.Success();

        repo.Delete(result);
        await repo.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}