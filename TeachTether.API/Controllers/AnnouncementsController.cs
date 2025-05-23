using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IAnnouncementService _announcementService;
        private readonly ITeacherService _teacherService;
        private readonly IGuardianService _guardianService;
        private readonly IClassGroupService _classGroupService;
        private readonly ISchoolService _schoolService;

        public AnnouncementsController(IAuthorizationService authorizationService, IAnnouncementService announcementService, ITeacherService teacherService, IGuardianService guardianService, IClassGroupService classGroupService, ISchoolService schoolService)
        {
            _authorizationService = authorizationService;
            _announcementService = announcementService;
            _teacherService = teacherService;
            _guardianService = guardianService;
            _classGroupService = classGroupService;
            _schoolService = schoolService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnnouncementResponse>>> GetAll()
        {
            var announcements = await _announcementService
                .GetAllForUserAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(announcements);
        }

        [HttpGet("/api/schools/{schoolId}/announcements")]
        [Authorize("RequireSchoolOwnerOrAdmin")]
        public async Task<ActionResult<IEnumerable<AnnouncementResponse>>> GetBySchool(int schoolId)
        {
            var school = await _schoolService.GetByIdAsync(schoolId);

            var announcements = await _announcementService
                .GetAllBySchoolId(schoolId);
            return Ok(announcements);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AnnouncementResponse>> Get(int id)
        {
            var announcement = await _announcementService.GetByIdAsync(id);

            var authResult = await _authorizationService.AuthorizeAsync(User, id, new CanViewAnnouncementRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            return Ok(announcement);
        }

        [HttpPost]
        [Authorize(Policy = "RequireTeacher")]
        public async Task<ActionResult<AnnouncementResponse>> Create([FromBody] CreateAnnouncementRequest request)
        {
            var teacherId = int.Parse(User.FindFirstValue("entity_id")!);
            var teacher = await _teacherService.GetByIdAsync(teacherId);
            foreach (var cgId in request.ClassGroupIds)
            {
                var classGroup = await _classGroupService.GetByIdAsync(cgId);
                if (classGroup.SchoolId != teacher.SchoolId)
                {
                    return Forbid();
                }
            }

            var authResult = await _authorizationService.AuthorizeAsync(User, request.ClassGroupIds, new CanCreateAnnouncementRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var announcement = await _announcementService.CreateAsync(request, teacherId);

            return CreatedAtAction(nameof(Get), new { id = announcement.Id }, announcement);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAnnouncementRequest request)
        {
            var announcement = await _announcementService.GetByIdAsync(id);

            var authResult = await _authorizationService.AuthorizeAsync(User, id, new CanManageAnnouncementRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _announcementService.UpdateAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var announcement = await _announcementService.GetByIdAsync(id);

            var authResult = await _authorizationService.AuthorizeAsync(User, id, new CanManageAnnouncementRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _announcementService.DeleteAsync(id);
            return NoContent();
        }
    }
}
