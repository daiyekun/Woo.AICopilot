using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Woo.AICopilot.Services.Common.Attributes;
using Woo.AICopilot.Services.Common.Contracts;
using Woo.AICopilot.SharedKernel.Messaging;
using Woo.AICopilot.SharedKernel.Result;

namespace Woo.AICopilot.AiGatewayService.Queries.ConversationTemplates;

[AuthorizeRequirement("AiGateway.GetConversationTemplate")]
public record GetConversationTemplateQuery(Guid Id) : IQuery<Result<ConversationTemplateDto>>;

public class GetConversationTemplateQueryHandler(
    IDataQueryService dataQueryService) : IQueryHandler<GetConversationTemplateQuery, Result<ConversationTemplateDto>>
{
    public async Task<Result<ConversationTemplateDto>> Handle(GetConversationTemplateQuery request,
        CancellationToken cancellationToken)
    {
        var queryable = dataQueryService.ConversationTemplates
            .Where(template => template.Id == request.Id)
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