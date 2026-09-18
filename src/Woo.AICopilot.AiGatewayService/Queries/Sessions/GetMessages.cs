using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Woo.AICopilot.Core.AiGateway.Aggregates.Sessions;
using Woo.AICopilot.Services.Common.Attributes;
using Woo.AICopilot.Services.Common.Contracts;
using Woo.AICopilot.SharedKernel.Messaging;
using Woo.AICopilot.SharedKernel.Result;

namespace Woo.AICopilot.AiGatewayService.Queries.Sessions;

public record MessageDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}

[AuthorizeRequirement("AiGateway.GetMessages")]
public record GetMessagesQuery(Guid SessionId) : IQuery<Result<IList<MessageDto>>>;

public class GetMessagesQueryHandler(
    IDataQueryService dataQueryService) : IQueryHandler<GetMessagesQuery, Result<IList<MessageDto>>>
{
    public async Task<Result<IList<MessageDto>>> Handle(GetMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var queryable = dataQueryService.Messages
            .Where(m => m.SessionId == request.SessionId)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                Content = m.Content,
                Type = m.Type,
                CreatedAt = m.CreatedAt
            });
        var result = await dataQueryService.ToListAsync(queryable);
        return Result.Success(result);
    }
}