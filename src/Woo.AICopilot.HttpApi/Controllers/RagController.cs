using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Woo.AICopilot.HttpApi.Infrastructure;
using Woo.AICopilot.RagService.Commands.Documents;
using Woo.AICopilot.RagService.Commands.KnowledgeBases;
using Woo.AICopilot.RagService.Queries.KnowledgeBases;

namespace Woo.AICopilot.HttpApi.Controllers;

[Route("/api/rag")]
[Authorize]
public class RagController : ApiControllerBase
{
    /// <summary>
    /// 创建知识库
    /// </summary>
    [HttpPost("knowledge-base")]
    public async Task<IActionResult> CreateKnowledgeBase(CreateKnowledgeBaseCommand command)
    {
        var result = await Sender.Send(command);
        return ReturnResult(result);
    }

    /// <summary>
    /// 上传文档
    /// </summary>
    [HttpPost("document")]
    [DisableRequestSizeLimit] // 允许上传大文件
    public async Task<IActionResult> UploadDocument(
        [FromForm] Guid knowledgeBaseId, 
        IFormFile file)
    {
        if (file.Length == 0)
        {
            return BadRequest(new { error = "请选择文件" });
        }

        // 将 IFormFile 转换为流
        await using var stream = file.OpenReadStream();
        
        var command = new UploadDocumentCommand(
            knowledgeBaseId, 
            new FileUploadStream(file.FileName, stream));

        var result = await Sender.Send(command);
        return ReturnResult(result);
    }
    
    [HttpPost("search")]
    public async Task<IActionResult> Search(SearchKnowledgeBaseQuery query)
    {
        var result = await Sender.Send(query);
        return ReturnResult(result);
    }
}