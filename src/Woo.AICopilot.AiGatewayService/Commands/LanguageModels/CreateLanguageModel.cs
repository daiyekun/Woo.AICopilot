using System;
using System.Threading;
using System.Threading.Tasks;
using Woo.AICopilot.Core.AiGateway.Aggregates.LanguageModel;
using Woo.AICopilot.Services.Common.Attributes;
using Woo.AICopilot.SharedKernel.Messaging;
using Woo.AICopilot.SharedKernel.Repository;
using Woo.AICopilot.SharedKernel.Result;

namespace Woo.AICopilot.AiGatewayService.Commands.LanguageModels;

public record CreatedLanguageModelDto(Guid Id, string Provider, string Name);

[AuthorizeRequirement("AiGateway.CreateLanguageModel")]
public record CreateLanguageModelCommand(
    string Provider,
    string Name,
    string BaseUrl,
    string? ApiKey,
    int MaxTokens,
    float Temperature = 0.7f) : ICommand<Result<CreatedLanguageModelDto>>;

public class CreateLanguageModelCommandHandler(IRepository<LanguageModel> repo)
    : ICommandHandler<CreateLanguageModelCommand, Result<CreatedLanguageModelDto>>
{
    public async Task<Result<CreatedLanguageModelDto>> Handle(CreateLanguageModelCommand request,
        CancellationToken cancellationToken)
    {
        var result = new LanguageModel(
            request.Name,
            request.Provider,
            request.BaseUrl,
            request.ApiKey,
            new ModelParameters
            {
                MaxTokens = request.MaxTokens,
                Temperature = request.Temperature
            });

        repo.Add(result);

        await repo.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreatedLanguageModelDto(result.Id, result.Provider, result.Name));
    }
}