using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SchoolsController(
    ISchoolService schoolService,
    IAuthorizationService authorizationService,
    IOwnerService ownerService) : ControllerBase
{
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IOwnerService _ownerService = ownerService;
    private readonly ISchoolService _schoolService = schoolService;

    [HttpGet("{id}")]
    public async Task<ActionResult<SchoolResponse>> Get(int id)
    {
        var school = await _schoolService.GetByIdAsync(id);

        var authResult = await _authorizationService.AuthorizeAsync(User, id, new CanViewSchoolRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        return Ok(school);
    }

    [HttpGet]
    [Authorize(Policy = "RequireSchoolOwner")]
    public async Task<ActionResult<IEnumerable<SchoolResponse>>> GetAll()
    {
        var schoolOwnerId = int.Parse(User.FindFirstValue("entity_id")!);
        if (!await _ownerService.ExistsAsync(schoolOwnerId))
            return Forbid();

        var schools = await _schoolService.GetAllByOwnerAsync(schoolOwnerId);
        return Ok(schools);
    }

    [HttpPost]
    [Authorize(Policy = "RequireSchoolOwner")]
    public async Task<ActionResult<SchoolResponse>> Create([FromBody] CreateSchoolRequest request)
    {
        var schoolOwnerId = int.Parse(User.FindFirstValue("entity_id")!);
        if (!await _ownerService.ExistsAsync(schoolOwnerId))
            return Forbid();
        var school = await _schoolService.CreateAsync(request, schoolOwnerId);
        return CreatedAtAction(nameof(Get), new { id = school.Id }, school);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "RequireSchoolOwner")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSchoolRequest request)
    {
        _ = await _schoolService.GetByIdAsync(id);

        var authResult = await _authorizationService.AuthorizeAsync(User, id, new CanManageSchoolRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        await _schoolService.UpdateAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireSchoolOwner")]
    public async Task<IActionResult> Delete(int id)
    {
        _ = await _schoolService.GetByIdAsync(id);

        var authResult = await _authorizationService.AuthorizeAsync(User, id, new CanManageSchoolRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        await _schoolService.DeleteAsync(id);
        return NoContent();
    }
}