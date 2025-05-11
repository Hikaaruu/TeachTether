using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers
{
    [Route("api/threads/{threadId}/messages/{messageId}/attachments")]
    [ApiController]
    [Authorize]
    public class MessageAttachmentsController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IMessageThreadService _messageThreadService;
        private readonly IMessageService _messageService;
        private readonly IMessageAttachmentService _messageAttachmentService;

        public MessageAttachmentsController(IAuthorizationService authorizationService, IMessageThreadService messageThreadService, IMessageService messageService, IMessageAttachmentService messageAttachmentService)
        {
            _authorizationService = authorizationService;
            _messageThreadService = messageThreadService;
            _messageService = messageService;
            _messageAttachmentService = messageAttachmentService;
        }

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
}
