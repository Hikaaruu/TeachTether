using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers
{
    [Route("api/schools/{schoolId}/[controller]")]
    [ApiController]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {

        private readonly IAnalyticsService _analyticsService;
        private readonly ISchoolService _schoolService;
        private readonly IStudentService _studentService;
        private readonly ISubjectService _subjectService;
        private readonly IClassGroupService _classGroupService;
        private readonly IAuthorizationService _authorizationService;

        public AnalyticsController(IAnalyticsService analyticsService, ISchoolService schoolService, IAuthorizationService authorizationService, IStudentService studentService, ISubjectService subjectService, IClassGroupService classGroupService)
        {
            _analyticsService = analyticsService;
            _schoolService = schoolService;
            _authorizationService = authorizationService;
            _studentService = studentService;
            _subjectService = subjectService;
            _classGroupService = classGroupService;
        }

        // ADD AUTH HERE
        [HttpGet("classgroups/{classGroupId}/subjects/{subjectId}/students/{studentId}/averages")]
        public async Task<ActionResult<ClassAveragesResponse>> Get(int schoolId, int classGroupId, int subjectId, int studentId)
        {
            var school = await _schoolService.GetByIdAsync(schoolId);
            var student = await _studentService.GetByIdAsync(studentId);
            var subject = await _subjectService.GetByIdAsync(subjectId);
            var classGroup = await _classGroupService.GetByIdAsync(classGroupId);
            if (student.SchoolId != schoolId ||
                subject.SchoolId != schoolId ||
                classGroup.SchoolId != schoolId)
                return NotFound();

            var averages = await _analyticsService.GetClassAverages(subjectId, studentId, classGroupId);

            return Ok(averages);
        }
    }
}
