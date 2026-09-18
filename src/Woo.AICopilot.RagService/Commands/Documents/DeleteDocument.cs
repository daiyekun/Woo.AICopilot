using Woo.AICopilot.Core.Rag.Aggregates.KnowledgeBase;
using Woo.AICopilot.Services.Common.Attributes;
using Woo.AICopilot.SharedKernel.Messaging;
using Woo.AICopilot.SharedKernel.Repository;
using Woo.AICopilot.SharedKernel.Result;

namespace Woo.AICopilot.RagService.Commands.Documents;

[AuthorizeRequirement("Rag.DeleteDocument")]
public record DeleteDocumentCommand(Guid KnowledgeBaseId, int DocumentId) : ICommand<Result>;

public class DeleteDocumentCommandHandler(IRepository<KnowledgeBase> repo)
    : ICommandHandler<DeleteDocumentCommand, Result>
{
    public async Task<Result> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var result = await repo.GetAsync(kb => kb.Id == request.KnowledgeBaseId,
            [k => k.Documents.Where(d => d.Id == request.DocumentId)], cancellationToken);
        if (result == null) return Result.Success();

        result.RemoveDocument(request.DocumentId);
        await repo.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}