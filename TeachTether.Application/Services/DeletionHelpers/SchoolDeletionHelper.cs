using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;

namespace TeachTether.Application.Services.DeletionHelpers;

public class SchoolDeletionHelper(
    IUnitOfWork unitOfWork,
    IStudentDeletionHelper studentDeletionHelper,
    ITeacherDeletionHelper teacherDeletionHelper,
    IGuardianDeletionHelper guardianDeletionHelper,
    ISchoolAdminDeletionHelper schoolAdminDeletionHelper,
    ISubjectDeletionHelper subjectDeletionHelper,
    IClassGroupDeletionHelper classGroupDeletionHelper) : ISchoolDeletionHelper
{
    private readonly IClassGroupDeletionHelper _classGroupDeletionHelper = classGroupDeletionHelper;
    private readonly IGuardianDeletionHelper _guardianDeletionHelper = guardianDeletionHelper;
    private readonly ISchoolAdminDeletionHelper _schoolAdminDeletionHelper = schoolAdminDeletionHelper;
    private readonly IStudentDeletionHelper _studentDeletionHelper = studentDeletionHelper;
    private readonly ISubjectDeletionHelper _subjectDeletionHelper = subjectDeletionHelper;
    private readonly ITeacherDeletionHelper _teacherDeletionHelper = teacherDeletionHelper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task DeleteSchoolAsync(int id)
    {
        var school = await _unitOfWork.Schools.GetByIdAsync(id)
                     ?? throw new NotFoundException("School Not Found");

        var subjects = await _unitOfWork.Subjects.GetBySchoolIdAsync(id);
        foreach (var subject in subjects) await _subjectDeletionHelper.DeleteSubjectAsync(subject.Id);

        var classGroups = await _unitOfWork.ClassGroups.GetBySchoolIdAsync(id);
        foreach (var classGroup in classGroups) await _classGroupDeletionHelper.DeleteClassGroupAsync(classGroup.Id);

        var students = await _unitOfWork.Students.GetBySchoolIdAsync(id);
        foreach (var s in students) await _studentDeletionHelper.DeleteStudentAsync(s.Id);

        var teachers = await _unitOfWork.Teachers.GetBySchoolIdAsync(id);
        foreach (var t in teachers) await _teacherDeletionHelper.DeleteTeacherAsync(t.Id);

        var guardians = await _unitOfWork.Guardians.GetBySchoolIdAsync(id);
        foreach (var g in guardians) await _guardianDeletionHelper.DeleteGuardianAsync(g.Id);

        var admins = await _unitOfWork.SchoolAdmins.GetBySchoolIdAsync(id);
        foreach (var a in admins) await _schoolAdminDeletionHelper.DeleteSchoolAdminAsync(a.Id);

        _unitOfWork.Schools.Delete(school);

        await _unitOfWork.SaveChangesAsync();
    }
}