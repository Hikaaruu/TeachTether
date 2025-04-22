using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.API.Controllers
{
    [Route("api/schools/{schoolId}/[controller]")]
    [ApiController]
    [Authorize(Policy = "RequireSchoolOwner")]
    public class SchoolAdminsController : ControllerBase
    {
        private readonly ISchoolAdminService _schoolAdminService;
        private readonly IAuthorizationService _authorizationService;

        public SchoolAdminsController(ISchoolAdminService schoolAdminService, IAuthorizationService authorizationService)
        {
            _schoolAdminService = schoolAdminService;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SchoolAdminResponse>>> GetAll(int schoolId)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var schoolAdmins = _schoolAdminService.GetAllBySchoolAsync(schoolId);
            return Ok(schoolAdmins);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SchoolAdminResponse>> Get(int id, int schoolId)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var schoolAdmin = await _schoolAdminService.GetByIdAsync(id);

            if (schoolAdmin.SchoolId != schoolId)
                return NotFound();

            return Ok(schoolAdmin);
        }
    }
}
