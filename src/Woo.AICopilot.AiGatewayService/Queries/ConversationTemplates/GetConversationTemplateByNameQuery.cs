using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Woo.AICopilot.Services.Common.Attributes;
using Woo.AICopilot.Services.Common.Contracts;
using Woo.AICopilot.SharedKernel.Messaging;
using Woo.AICopilot.SharedKernel.Result;

namespace Woo.AICopilot.AiGatewayService.Queries.ConversationTemplates;

[AuthorizeRequirement("AiGateway.GetConversationTemplateByName")]
public record GetConversationTemplateByNameQuery(string Name) : IQuery<Result<ConversationTemplateDto>>;

public class GetConversationTemplateByNameQueryHandler(
    IDataQueryService dataQueryService) : IQueryHandler<GetConversationTemplateByNameQuery, Result<ConversationTemplateDto>>
{
    public async Task<Result<ConversationTemplateDto>> Handle(GetConversationTemplateByNameQuery request,
        CancellationToken cancellationToken)
    {
        var queryable = dataQueryService.ConversationTemplates
            .Where(template => template.Name == request.Name)
            .Select(ct => new ConversationTemplateDto
            {
                Id = ct.Id,
                Name = ct.Name,
                Description = ct.Description,
                SystemPrompt = ct.SystemPrompt,
                MaxTokens = ct.Specification.MaxTokens,
                Temperature = ct.Specification.Temperature
            });
        var result = await dataQueryService.FirstOrDefaultAsync(queryable);

        return result == null ? Result.NotFound() : Result.Success(result);
    }
}