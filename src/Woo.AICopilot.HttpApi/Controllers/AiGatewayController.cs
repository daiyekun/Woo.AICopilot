using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Woo.AICopilot.AiGatewayService.Agents;
using Woo.AICopilot.AiGatewayService.Commands.ConversationTemplates;
using Woo.AICopilot.AiGatewayService.Commands.LanguageModels;
using Woo.AICopilot.AiGatewayService.Commands.Sessions;
using Woo.AICopilot.AiGatewayService.Queries.ConversationTemplates;
using Woo.AICopilot.AiGatewayService.Queries.LanguageModels;
using Woo.AICopilot.AiGatewayService.Queries.Sessions;
using Woo.AICopilot.HttpApi.Infrastructure;

namespace Woo.AICopilot.HttpApi.Controllers;

[Route("/api/aigateway")]
[Authorize] // 默认开启认证
public class AiGatewayController : ApiControllerBase
{
    [HttpPost("language-model")]
    public async Task<IActionResult> CreateLanguageModel(CreateLanguageModelCommand command)
    {
        var result = await Sender.Send(command);

        return ReturnResult(result);
    }

    [HttpDelete("language-model")]
    public async Task<IActionResult> DeleteLanguageModel(DeleteLanguageModelCommand command)
    {
        var result = await Sender.Send(command);

        return ReturnResult(result);
    }

    [HttpGet("language-model/list")]
    public async Task<IActionResult> GetListLanguageModels()
    {
        var result = await Sender.Send(new GetListLanguageModelsQuery());
        return ReturnResult(result);
    }

    [HttpPost("conversation-template")]
    public async Task<IActionResult> CreateConversationTemplate(CreateConversationTemplateCommand command)
    {
        var result = await Sender.Send(command);
        return ReturnResult(result);
    }

    [HttpDelete("conversation-template")]
    public async Task<IActionResult> DeleteConversationTemplate(DeleteConversationTemplateCommand command)
    {
        var result = await Sender.Send(command);
        return ReturnResult(result);
    }

    [HttpGet("conversation-template")]
    public async Task<IActionResult> GetConversationTemplate(GetConversationTemplateQuery query)
    {
        var result = await Sender.Send(query);
        return ReturnResult(result);
    }

    [HttpGet("conversation-template/list")]
    public async Task<IActionResult> GetListConversationTemplates()
    {
        var result = await Sender.Send(new GetListConversationTemplatesQuery());
        return ReturnResult(result);
    }

    [HttpPost("session")]
    public async Task<IActionResult> CreateSession(CreateSessionCommand command)
    {
        var result = await Sender.Send(command);
        return ReturnResult(result);
    }

    [HttpDelete("session")]
    public async Task<IActionResult> DeleteSession(DeleteSessionCommand command)
    {
        var result = await Sender.Send(command);
        return ReturnResult(result);
    }

    [HttpGet("session/list")]
    public async Task<IActionResult> GetListSessions()
    {
        var result = await Sender.Send(new GetListSessionsQuery());
        return ReturnResult(result);
    }
    
    [HttpPost("chat")]
    public IResult Chat(ChatStreamRequest request)
    {
        var stream = Sender.CreateStream(request);
        return Results.ServerSentEvents(stream);
    }
    
    [HttpGet("messages")]
    public async Task<IActionResult> GetMessages(Guid sessionId)
    {
        var result = await Sender.Send(new GetMessagesQuery(sessionId));
        return ReturnResult(result);
    }
}