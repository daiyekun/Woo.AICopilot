using System;
using System.Threading;
using System.Threading.Tasks;
using Woo.AICopilot.Core.AiGateway.Aggregates.ConversationTemplate;
using Woo.AICopilot.Services.Common.Attributes;
using Woo.AICopilot.SharedKernel.Messaging;
using Woo.AICopilot.SharedKernel.Repository;
using Woo.AICopilot.SharedKernel.Result;

namespace Woo.AICopilot.AiGatewayService.Commands.ConversationTemplates;

public record CreatedConversationTemplateDto(Guid Id, string Name);

[AuthorizeRequirement("AiGateway.CreateConversationTemplate")]
public record CreateConversationTemplateCommand(
    string Name,
    string Description,
    string SystemPrompt,
    Guid ModelId,
    int? MaxTokens,
    float? Temperature) : ICommand<Result<CreatedConversationTemplateDto>>;

public class CreateConversationTemplateCommandHandler(IRepository<ConversationTemplate> repo)
    : ICommandHandler<CreateConversationTemplateCommand, Result<CreatedConversationTemplateDto>>
{
    public async Task<Result<CreatedConversationTemplateDto>> Handle(CreateConversationTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var model = new ConversationTemplate(
            request.Name,
            request.Description,
            request.SystemPrompt,
            request.ModelId,
            new TemplateSpecification
            {
                MaxTokens = request.MaxTokens,
                Temperature = request.Temperature
            });

        repo.Add(model);

        await repo.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreatedConversationTemplateDto(model.Id, model.Name));
    }
}