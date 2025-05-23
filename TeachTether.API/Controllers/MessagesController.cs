using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        private readonly ITeacherService _teacherService;
        private readonly IGuardianService _guardianService;

        public MessagesController(IAuthorizationService authorizationService, IMessageThreadService messageThreadService, ITeacherService teacherService, IGuardianService guardianService, IMessageService messageService)
        {
            _authorizationService = authorizationService;
            _messageThreadService = messageThreadService;
            _teacherService = teacherService;
            _guardianService = guardianService;
            _messageService = messageService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageResponse>>> GetAll(int threadId)
        {

            var thread = await _messageThreadService.GetByIdAsync(threadId);

            var authResult = await _authorizationService.AuthorizeAsync(User, threadId, new CanViewThreadRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var messages = await _messageService.GetAllAsync(threadId);
            return Ok(messages);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MessageResponse>> Get(int threadId, int id)
        {
            var thread = await _messageThreadService.GetByIdAsync(threadId);

            var authResult = await _authorizationService.AuthorizeAsync(User, threadId, new CanViewThreadRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var message = await _messageService.GetByIdAsync(id);

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
            return NoContent();
        }

    }
}
