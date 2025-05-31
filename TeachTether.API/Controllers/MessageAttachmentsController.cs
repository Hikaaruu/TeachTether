using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers;

[Route("api/threads/{threadId}/messages/{messageId}/attachments")]
[ApiController]
[Authorize]
public class MessageAttachmentsController(
    IAuthorizationService authorizationService,
    IMessageThreadService messageThreadService,
    IMessageService messageService,
    IMessageAttachmentService messageAttachmentService) : ControllerBase
{
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMessageAttachmentService _messageAttachmentService = messageAttachmentService;
    private readonly IMessageService _messageService = messageService;
    private readonly IMessageThreadService _messageThreadService = messageThreadService;

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id, int threadId, int messageId)
    {
        var thread = await _messageThreadService.GetByIdAsync(threadId);
        var message = await _messageService.GetByIdAsync(messageId);

        if (message.ThreadId != thread.Id)
            return NotFound();

        var file = await _messageAttachmentService.GetFileByIdAsync(id);

        var authResult = await _authorizationService.AuthorizeAsync(User, id, new CanViewThreadRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        return File(file.Content, file.ContentType, file.FileName);
    }
}