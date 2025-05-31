using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers;

[Route("api/threads")]
[ApiController]
[Authorize]
public class MessageThreadsController(
    IAuthorizationService authorizationService,
    IMessageThreadService messageThreadService,
    ITeacherService teacherService,
    IGuardianService guardianService) : ControllerBase
{
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IGuardianService _guardianService = guardianService;
    private readonly IMessageThreadService _messageThreadService = messageThreadService;
    private readonly ITeacherService _teacherService = teacherService;

    [HttpGet]
    [Authorize(Policy = "RequireTeacherOrGuardian")]
    public async Task<ActionResult<IEnumerable<MessageThreadResponse>>> GetAll()
    {
        var threads = await _messageThreadService
            .GetAllForUserAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(threads);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MessageThreadResponse>> Get(int id)
    {
        var thread = await _messageThreadService.GetByIdAsync(id);

        var authResult = await _authorizationService.AuthorizeAsync(User, id, new CanViewThreadRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        return Ok(thread);
    }

    [HttpPost]
    public async Task<ActionResult<MessageThreadResponse>> Create([FromBody] CreateMessageThreadRequest request)
    {
        var teacher = await _teacherService.GetByIdAsync(request.TeacherId);
        var guardian = await _guardianService.GetByIdAsync(request.GuardianId);
        if (teacher.SchoolId != guardian.SchoolId)
            return BadRequest();

        var authResult = await _authorizationService.AuthorizeAsync(User, (request.TeacherId, request.GuardianId),
            new CanCreateThreadRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        var thread = await _messageThreadService.CreateAsync(request);

        return CreatedAtAction(nameof(Get), new { id = thread.Id }, thread);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var thread = await _messageThreadService.GetByIdAsync(id);

        var authResult = await _authorizationService.AuthorizeAsync(User, thread, new CanDeleteThreadRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        await _messageThreadService.DeleteAsync(id);
        return NoContent();
    }
}