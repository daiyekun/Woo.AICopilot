using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Woo.AICopilot.Services.Common.Attributes;
using Woo.AICopilot.Services.Common.Contracts;
using Woo.AICopilot.SharedKernel.Messaging;
using Woo.AICopilot.SharedKernel.Result;

namespace Woo.AICopilot.AiGatewayService.Queries.Sessions;

public record SessionDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
}

[AuthorizeRequirement("AiGateway.GetListSessions")]
public record GetListSessionsQuery : IQuery<Result<IList<SessionDto>>>;

public class GetListSessionsQueryHandler(
    IDataQueryService dataQueryService) : IQueryHandler<GetListSessionsQuery, Result<IList<SessionDto>>>
{
    public async Task<Result<IList<SessionDto>>> Handle(GetListSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var queryable = dataQueryService.Sessions
            .OrderByDescending(s => s.Id)
            .Select(s => new SessionDto
            {
                Id = s.Id,
                Title = s.Title
            });
        var result = await dataQueryService.ToListAsync(queryable);
        return Result.Success(result);
    }
}