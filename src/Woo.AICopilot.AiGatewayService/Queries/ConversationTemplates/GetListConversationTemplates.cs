using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Woo.AICopilot.Services.Common.Attributes;
using Woo.AICopilot.Services.Common.Contracts;
using Woo.AICopilot.SharedKernel.Messaging;
using Woo.AICopilot.SharedKernel.Result;

namespace Woo.AICopilot.AiGatewayService.Queries.ConversationTemplates;

[AuthorizeRequirement("AiGateway.GetListConversationTemplates")]
public record GetListConversationTemplatesQuery : IQuery<Result<IList<ConversationTemplateDto>>>;

public class GetListConversationTemplatesQueryHandler(
    IDataQueryService dataQueryService)
    : IQueryHandler<GetListConversationTemplatesQuery, Result<IList<ConversationTemplateDto>>>
{
    public async Task<Result<IList<ConversationTemplateDto>>> Handle(GetListConversationTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var queryable = dataQueryService.ConversationTemplates
            .Select(ct => new ConversationTemplateDto
            {
                Id = ct.Id,
                Name = ct.Name,
                Description = ct.Description,
                SystemPrompt = ct.SystemPrompt,
                MaxTokens = ct.Specification.MaxTokens,
                Temperature = ct.Specification.Temperature
            });
        var result = await dataQueryService.ToListAsync(queryable);
        return Result.Success(result);
    }
}