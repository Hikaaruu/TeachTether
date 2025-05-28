using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TeachTether.API.Hubs;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers
{
    [Route("api/threads/{threadId}/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IMessageThreadService _messageThreadService;
        private readonly IMessageService _messageService;
        private readonly IHubContext<ChatHub> _hub;

        public MessagesController(IAuthorizationService authorizationService, IMessageThreadService messageThreadService, IMessageService messageService, IHubContext<ChatHub> hub)
        {
            _authorizationService = authorizationService;
            _messageThreadService = messageThreadService;
            _messageService = messageService;
            _hub = hub;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageResponse>>> GetAll(
            int threadId,
            [FromQuery] int take = 50,
            [FromQuery] int? beforeId = null)
        {

            var thread = await _messageThreadService.GetByIdAsync(threadId);
            if (beforeId.HasValue)
            {
                var message = await _messageService.GetByIdAsync(beforeId.Value);
                if (message is null || message.ThreadId != threadId)
                {
                    return BadRequest();
                }
            }

            var authResult = await _authorizationService.AuthorizeAsync(User, threadId, new CanViewThreadRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var messages = await _messageService.GetByThreadAsync(threadId, take, beforeId);
            return Ok(messages);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MessageResponse>> Get(int threadId, int id)
        {
            var thread = await _messageThreadService.GetByIdAsync(threadId);
            var message = await _messageService.GetByIdAsync(id);
            if (message.ThreadId != threadId)
                return Forbid();

            var authResult = await _authorizationService.AuthorizeAsync(User, threadId, new CanViewThreadRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            return Ok(message);
        }

        [HttpPost]
        [Authorize(Policy = "RequireTeacherOrGuardian")]
        public async Task<ActionResult<MessageResponse>> Create([FromBody] CreateMessageRequest request, int threadId)
        {
            var thread = await _messageThreadService.GetByIdAsync(threadId);

            var authResult = await _authorizationService.AuthorizeAsync(User, thread, new CanCreateMessageRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var message = await _messageService
                .CreateAsync(request, threadId, User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _hub.Clients
              .Group($"thread-{threadId}")
              .SendAsync("ReceiveMessage", message);


            return CreatedAtAction(nameof(Get), new { id = message.Id, threadId }, message);
        }

        [HttpPatch("{id}")]
        [Authorize(Policy = "RequireTeacherOrGuardian")]
        public async Task<IActionResult> MarkRead(int threadId, int id)
        {
            var thread = await _messageThreadService.GetByIdAsync(threadId);

            var authResult = await _authorizationService.AuthorizeAsync(User, threadId, new CanViewThreadRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _messageService.ReadAsync(id);

            await _hub.Clients
                .Group($"thread-{threadId}")
                .SendAsync("MessageRead", id);       
            
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, int threadId)
        {
            var thread = await _messageThreadService.GetByIdAsync(threadId);

            var authResult = await _authorizationService.AuthorizeAsync(User, thread, new CanDeleteThreadRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _messageService.DeleteAsync(id);
            await _hub.Clients.Group($"thread-{threadId}").SendAsync("MessageDeleted", id);
            return NoContent();
        }

    }
}
