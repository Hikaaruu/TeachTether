using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services;

public class AnnouncementService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IUserService userService,
    IAnnouncementDeletionHelper announcementDeletionHelper) : IAnnouncementService
{
    private readonly IAnnouncementDeletionHelper _announcementDeletionHelper = announcementDeletionHelper;
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserService _userService = userService;

    public async Task<AnnouncementResponse> CreateAsync(CreateAnnouncementRequest request, int teacherId)
    {
        var announcement = _mapper.Map<Announcement>(request);
        announcement.TeacherId = teacherId;
        announcement.CreatedAt = DateTime.Now;

        await _unitOfWork.Announcements.AddAsync(announcement);
        await _unitOfWork.SaveChangesAsync();

        foreach (var item in request.ClassGroupIds)
        {
            var announcementClassGroup = new AnnouncementClassGroup
            {
                AnnouncementId = announcement.Id,
                ClassGroupId = item
            };
            await _unitOfWork.AnnouncementClassGroups.AddAsync(announcementClassGroup);
        }

        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<AnnouncementResponse>(announcement);
    }

    public async Task DeleteAsync(int id)
    {
        await _announcementDeletionHelper.DeleteAnnouncementAsync(id);
    }

    public async Task<IEnumerable<AnnouncementResponse>> GetAllBySchoolId(int schoolId)
    {
        var announcements = await _unitOfWork.Announcements.GetAllAsync();
        var result = new List<Announcement>();

        foreach (var announcement in announcements)
        {
            var teacher = await _unitOfWork.Teachers.GetByIdAsync(announcement.TeacherId);
            if (teacher != null && teacher.SchoolId == schoolId) result.Add(announcement);
        }

        return _mapper.Map<IEnumerable<AnnouncementResponse>>(result);
    }


    public async Task<IEnumerable<AnnouncementResponse>> GetAllForUserAsync(string userId)
    {
        var user = await _userService.GetByIdAsync(userId)
                   ?? throw new NotFoundException("User not found");

        IEnumerable<Announcement> announcements;

        switch (user.UserType)
        {
            case UserType.Teacher:
            {
                var teacher = await _unitOfWork.Teachers.GetByUserIdAsync(userId)
                              ?? throw new NotFoundException("Teacher not found");

                announcements = await _unitOfWork.Announcements.GetByTeacherIdAsync(teacher.Id);
                break;
            }

            case UserType.Guardian:
            {
                var guardian = await _unitOfWork.Guardians.GetByUserIdAsync(userId)
                               ?? throw new NotFoundException("Guardian not found");

                var studentIds = (await _unitOfWork.GuardianStudents
                        .GetByGuardianIdAsync(guardian.Id))
                    .Select(gs => gs.StudentId);

                var classGroupIds = (await _unitOfWork.ClassGroupStudents
                        .GetAllAsync(cgs => studentIds.Contains(cgs.StudentId)))
                    .Select(cgs => cgs.ClassGroupId);

                var announcementIds = (await _unitOfWork.AnnouncementClassGroups
                        .GetAllAsync(acg => classGroupIds.Contains(acg.ClassGroupId)))
                    .Select(acg => acg.AnnouncementId);

                announcements = (await _unitOfWork.Announcements.GetByIdsAsync(announcementIds))
                    .Where(a => a.TargetAudience == AudienceType.Guardian ||
                                a.TargetAudience == AudienceType.StudentAndGuardian);

                break;
            }

            case UserType.Student:
            {
                var student = await _unitOfWork.Students.GetByUserIdAsync(userId)
                              ?? throw new NotFoundException("Student not found");

                var classGroupStudent = await _unitOfWork.ClassGroupStudents
                    .GetByStudentIdAsync(student.Id);

                if (classGroupStudent is null)
                {
                    announcements = new List<Announcement>();
                    break;
                }

                var announcementIds = (await _unitOfWork.AnnouncementClassGroups
                        .GetByClassGroupIdAsync(classGroupStudent.ClassGroupId))
                    .Select(acg => acg.AnnouncementId);

                announcements = (await _unitOfWork.Announcements.GetByIdsAsync(announcementIds))
                    .Where(a => a.TargetAudience == AudienceType.Student ||
                                a.TargetAudience == AudienceType.StudentAndGuardian);

                break;
            }

            default:
                throw new Exception("Unexpected behavior occurred");
        }

        return _mapper.Map<IEnumerable<AnnouncementResponse>>(announcements);
    }

    public async Task<AnnouncementResponse> GetByIdAsync(int id)
    {
        var announcement = await _unitOfWork.Announcements.GetByIdAsync(id)
                           ?? throw new NotFoundException("Announcement not found");

        return _mapper.Map<AnnouncementResponse>(announcement);
    }

    public async Task UpdateAsync(int id, UpdateAnnouncementRequest request)
    {
        var announcement = await _unitOfWork.Announcements.GetByIdAsync(id)
                           ?? throw new NotFoundException("Announcement not found");

        _mapper.Map(request, announcement);
        _unitOfWork.Announcements.Update(announcement);
        await _unitOfWork.SaveChangesAsync();
    }
}