using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;

namespace TeachTether.Application.Services.DeletionHelpers;

public class TeacherDeletionHelper(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IAnnouncementDeletionHelper announcementDeletionHelper,
    IMessageThreadDeletionHelper messageThreadDeletionHelper,
    IClassGroupDeletionHelper classGroupDeletionHelper) : ITeacherDeletionHelper
{
    private readonly IAnnouncementDeletionHelper _announcementDeletionHelper = announcementDeletionHelper;
    private readonly IClassGroupDeletionHelper _classGroupDeletionHelper = classGroupDeletionHelper;
    private readonly IMessageThreadDeletionHelper _messageThreadDeletionHelper = messageThreadDeletionHelper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task DeleteTeacherAsync(int id)
    {
        var teacher = await _unitOfWork.Teachers.GetByIdAsync(id)
                      ?? throw new NotFoundException("Teacher not found");

        var cg = await _unitOfWork.ClassGroups.GetByHomeroomTeacherIdAsync(id);
        if (cg.Any())
            throw new BadRequestException("Teacher is assigned to one of class groups as homeroom teacher");

        var grades = await _unitOfWork.StudentGrades.GetAllAsync(g => g.TeacherId == id);
        _unitOfWork.StudentGrades.DeleteMany(grades);
        var att = await _unitOfWork.StudentAttendances.GetAllAsync(g => g.TeacherId == id);
        _unitOfWork.StudentAttendances.DeleteMany(att);
        var beh = await _unitOfWork.StudentBehaviors.GetAllAsync(g => g.TeacherId == id);
        _unitOfWork.StudentBehaviors.DeleteMany(beh);

        var classAss = await _unitOfWork.ClassAssignments.GetByTeacherIdAsync(id);
        _unitOfWork.ClassAssignments.DeleteMany(classAss);

        var announcements = await _unitOfWork.Announcements.GetByTeacherIdAsync(id);
        foreach (var a in announcements) await _announcementDeletionHelper.DeleteAnnouncementAsync(a.Id);

        var threads = await _unitOfWork.MessageThreads.GetByTeacherIdAsync(id);
        foreach (var t in threads) await _messageThreadDeletionHelper.DeleteMessageThreadAsync(t.Id);

        _unitOfWork.Teachers.Delete(teacher);
        var result = await _userRepository.DeleteAsync(teacher.UserId);

        if (result.Succeeded)
            await _unitOfWork.SaveChangesAsync();
        else
            throw new Exception();
    }
}