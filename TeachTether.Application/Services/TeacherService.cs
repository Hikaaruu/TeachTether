using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services;

public class TeacherService(
    IUnitOfWork unitOfWork,
    IUserService userService,
    IMapper mapper,
    ITeacherDeletionHelper teacherDeletionHelper) : ITeacherService
{
    private readonly IMapper _mapper = mapper;
    private readonly ITeacherDeletionHelper _teacherDeletionHelper = teacherDeletionHelper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserService _userService = userService;

    public async Task<CreatedTeacherResponse> CreateAsync(CreateTeacherRequest request, int schoolId)
    {
        var (user, password) = await _userService.CreateAsync(request.User, UserType.Teacher);

        var teacher = new Teacher
        {
            UserId = user.Id!,
            DateOfBirth = request.DateOfBirth,
            SchoolId = schoolId
        };

        await _unitOfWork.Teachers.AddAsync(teacher);
        await _unitOfWork.SaveChangesAsync();

        return new CreatedTeacherResponse
        {
            User = _mapper.Map<UserDto>(user),
            Id = teacher.Id,
            Username = user.UserName,
            Password = password,
            SchoolId = teacher.SchoolId,
            DateOfBirth = teacher.DateOfBirth
        };
    }

    public async Task DeleteAsync(int id)
    {
        await _teacherDeletionHelper.DeleteTeacherAsync(id);
    }

    public async Task<IEnumerable<TeacherResponse>> GetAllByClassGroupSubjectAsync(int classGroupId, int subjectId)
    {
        var classGroupSubject = (await _unitOfWork.ClassGroupsSubjects
                                    .GetByClassGroupIdAsync(classGroupId))
                                .SingleOrDefault(cgs => cgs.SubjectId == subjectId)
                                ?? throw new BadRequestException(
                                    $"Subject {subjectId} is not assigned to class group {classGroupId}");

        var teachersIds = (await _unitOfWork.ClassAssignments
                .GetByClassGroupSubjectIdAsync(classGroupSubject.Id))
            .Select(ca => ca.TeacherId);

        var teachers = await _unitOfWork.Teachers.GetByIdsAsync(teachersIds);

        var userIds = teachers.Select(s => s.UserId);
        var users = await _userService.GetByIdsAsync(userIds);
        var userMap = users
            .Where(u => u.Id != null)
            .ToDictionary(u => u.Id!);

        var result = new List<TeacherResponse>();

        foreach (var teacher in teachers)
        {
            if (!userMap.TryGetValue(teacher.UserId, out var user))
                throw new Exception($"User data can not be found for teacher with id = {teacher.Id}");

            var response = new TeacherResponse
            {
                User = _mapper.Map<UserDto>(user),
                Id = teacher.Id,
                SchoolId = teacher.SchoolId,
                DateOfBirth = teacher.DateOfBirth
            };

            result.Add(response);
        }

        return result;
    }

    public async Task<IEnumerable<TeacherResponse>> GetAllBySchoolAsync(int schoolId)
    {
        var teachers = await _unitOfWork.Teachers.GetBySchoolIdAsync(schoolId);

        var userIds = teachers.Select(s => s.UserId);
        var users = await _userService.GetByIdsAsync(userIds);
        var userMap = users
            .Where(u => u.Id != null)
            .ToDictionary(u => u.Id!);

        var result = new List<TeacherResponse>();

        foreach (var teacher in teachers)
        {
            if (!userMap.TryGetValue(teacher.UserId, out var user))
                throw new Exception($"User data can not be found for teacher with id = {teacher.Id}");

            var response = new TeacherResponse
            {
                User = _mapper.Map<UserDto>(user),
                Id = teacher.Id,
                SchoolId = teacher.SchoolId,
                DateOfBirth = teacher.DateOfBirth
            };

            result.Add(response);
        }

        return result;
    }

    public async Task<IEnumerable<TeacherResponse>> GetAvailableForGuardianAsync(int guardianId)
    {
        var studentIds = (await _unitOfWork.GuardianStudents
                .GetByGuardianIdAsync(guardianId))
            .Select(gs => gs.StudentId);

        var classGroupIds = (await _unitOfWork.ClassGroupStudents
                .GetAllAsync(cgs => studentIds.Contains(cgs.StudentId)))
            .Select(cgs => cgs.ClassGroupId);

        var hmTeacherIds = (await _unitOfWork.ClassGroups.GetByIdsAsync(classGroupIds))
            .Select(cg => cg.HomeroomTeacherId);

        var classGroupSubjectIds = (await _unitOfWork.ClassGroupsSubjects
                .GetAllAsync(cgs => classGroupIds.Contains(cgs.ClassGroupId)))
            .Select(cgs => cgs.Id);

        var teacherIds = (await _unitOfWork.ClassAssignments
                .GetAllAsync(ca => classGroupSubjectIds.Contains(ca.ClassGroupSubjectId)))
            .Select(ca => ca.TeacherId);

        var teachers = await _unitOfWork.Teachers.GetByIdsAsync(hmTeacherIds.Union(teacherIds));

        var userIds = teachers.Select(s => s.UserId);
        var users = await _userService.GetByIdsAsync(userIds);
        var userMap = users
            .Where(u => u.Id != null)
            .ToDictionary(u => u.Id!);

        var result = new List<TeacherResponse>();

        foreach (var teacher in teachers)
        {
            if (!userMap.TryGetValue(teacher.UserId, out var user))
                throw new Exception($"User data can not be found for teacher with id = {teacher.Id}");

            var response = new TeacherResponse
            {
                User = _mapper.Map<UserDto>(user),
                Id = teacher.Id,
                SchoolId = teacher.SchoolId,
                DateOfBirth = teacher.DateOfBirth
            };

            result.Add(response);
        }

        return result;
    }

    public async Task<TeacherResponse> GetByIdAsync(int id)
    {
        var teacher = await _unitOfWork.Teachers.GetByIdAsync(id)
                      ?? throw new NotFoundException("Teacher not found");

        var user = await _userService.GetByIdAsync(teacher.UserId);

        return new TeacherResponse
        {
            User = _mapper.Map<UserDto>(user),
            Id = teacher.Id,
            SchoolId = teacher.SchoolId,
            DateOfBirth = teacher.DateOfBirth
        };
    }

    public async Task UpdateAsync(int id, UpdateTeacherRequest request)
    {
        var teacher = await _unitOfWork.Teachers.GetByIdAsync(id)
                      ?? throw new NotFoundException("Teacher not found");

        await _userService.UpdateAsync(teacher.UserId, request.User);

        teacher.DateOfBirth = request.DateOfBirth;
        _unitOfWork.Teachers.Update(teacher);
        await _unitOfWork.SaveChangesAsync();
    }
}