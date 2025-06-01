using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services;

public class GuardianService(
    IUnitOfWork unitOfWork,
    IUserService userService,
    IMapper mapper,
    IGuardianDeletionHelper guardianDeletionHelper) : IGuardianService
{
    private readonly IGuardianDeletionHelper _guardianDeletionHelper = guardianDeletionHelper;
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserService _userService = userService;

    public async Task<CreatedGuardianResponse> CreateAsync(CreateGuardianRequest request, int schoolId)
    {
        var (user, password) = await _userService.CreateAsync(request.User, UserType.Guardian);

        var guardian = new Guardian
        {
            UserId = user.Id!,
            DateOfBirth = request.DateOfBirth,
            SchoolId = schoolId
        };

        await _unitOfWork.Guardians.AddAsync(guardian);
        await _unitOfWork.SaveChangesAsync();

        return new CreatedGuardianResponse
        {
            User = _mapper.Map<UserDto>(user),
            Id = guardian.Id,
            Username = user.UserName,
            Password = password,
            SchoolId = guardian.SchoolId,
            DateOfBirth = guardian.DateOfBirth
        };
    }

    public async Task DeleteAsync(int id)
    {
        await _guardianDeletionHelper.DeleteGuardianAsync(id);
    }

    public async Task<IEnumerable<GuardianResponse>> GetAllBySchoolAsync(int schoolId)
    {
        var guardians = await _unitOfWork.Guardians.GetBySchoolIdAsync(schoolId);

        var userIds = guardians.Select(s => s.UserId);
        var users = await _userService.GetByIdsAsync(userIds);
        var userMap = users
            .Where(u => u.Id != null)
            .ToDictionary(u => u.Id!);

        var result = new List<GuardianResponse>();

        foreach (var guardian in guardians)
        {
            if (!userMap.TryGetValue(guardian.UserId, out var user))
                throw new Exception($"User data can not be found for guardian with id = {guardian.Id}");

            var response = new GuardianResponse
            {
                User = _mapper.Map<UserDto>(user),
                Id = guardian.Id,
                SchoolId = guardian.SchoolId,
                DateOfBirth = guardian.DateOfBirth
            };

            result.Add(response);
        }

        return result;
    }

    public async Task<IEnumerable<GuardianResponse>> GetAllByStudentAsync(int studentId)
    {
        var guardianIds = (await _unitOfWork.GuardianStudents
                .GetByStudentIdAsync(studentId))
            .Select(gs => gs.GuardianId);

        var guardians = await _unitOfWork.Guardians.GetByIdsAsync(guardianIds);

        var userIds = guardians.Select(s => s.UserId);
        var users = await _userService.GetByIdsAsync(userIds);
        var userMap = users
            .Where(u => u.Id != null)
            .ToDictionary(u => u.Id!);

        var result = new List<GuardianResponse>();

        foreach (var guardian in guardians)
        {
            if (!userMap.TryGetValue(guardian.UserId, out var user))
                throw new Exception($"User data can not be found for guardian with id = {guardian.Id}");

            var response = new GuardianResponse
            {
                User = _mapper.Map<UserDto>(user),
                Id = guardian.Id,
                SchoolId = guardian.SchoolId,
                DateOfBirth = guardian.DateOfBirth
            };

            result.Add(response);
        }

        return result;
    }

    public async Task<IEnumerable<GuardianResponse>> GetAvailableForTeacherAsync(int teacherId)
    {
        var homeClassGroupIds = (await _unitOfWork.ClassGroups
                .GetByHomeroomTeacherIdAsync(teacherId))
            .Select(cg => cg.Id);

        var classGroupSubjectIds = (await _unitOfWork.ClassAssignments
                .GetByTeacherIdAsync(teacherId))
            .Select(ca => ca.ClassGroupSubjectId);

        var subjectClassGroupIds = (await _unitOfWork.ClassGroupsSubjects
                .GetByIdsAsync(classGroupSubjectIds))
            .Select(cgs => cgs.ClassGroupId);

        var allClassGroupIds = homeClassGroupIds.Union(subjectClassGroupIds).Distinct();

        var studentIds = (await _unitOfWork.ClassGroupStudents
                .GetAllAsync(cgs => allClassGroupIds.Contains(cgs.ClassGroupId)))
            .Select(cgs => cgs.StudentId)
            .Distinct();

        var guardianIds = (await _unitOfWork.GuardianStudents
                .GetAllAsync(gs => studentIds.Contains(gs.StudentId)))
            .Select(gs => gs.GuardianId)
            .Distinct();

        var guardians = await _unitOfWork.Guardians.GetByIdsAsync(guardianIds);

        var userIds = guardians.Select(s => s.UserId);
        var users = await _userService.GetByIdsAsync(userIds);
        var userMap = users
            .Where(u => u.Id != null)
            .ToDictionary(u => u.Id!);

        var result = new List<GuardianResponse>();

        foreach (var guardian in guardians)
        {
            if (!userMap.TryGetValue(guardian.UserId, out var user))
                throw new Exception($"User data can not be found for guardian with id = {guardian.Id}");

            var response = new GuardianResponse
            {
                User = _mapper.Map<UserDto>(user),
                Id = guardian.Id,
                SchoolId = guardian.SchoolId,
                DateOfBirth = guardian.DateOfBirth
            };

            result.Add(response);
        }

        return result;
    }


    public async Task<GuardianResponse> GetByIdAsync(int id)
    {
        var guardian = await _unitOfWork.Guardians.GetByIdAsync(id)
                       ?? throw new NotFoundException("Guardian not found");

        var user = await _userService.GetByIdAsync(guardian.UserId);

        return new GuardianResponse
        {
            User = _mapper.Map<UserDto>(user),
            Id = guardian.Id,
            SchoolId = guardian.SchoolId,
            DateOfBirth = guardian.DateOfBirth
        };
    }

    public async Task UpdateAsync(int id, UpdateGuardianRequest request)
    {
        var guardian = await _unitOfWork.Guardians.GetByIdAsync(id)
                       ?? throw new NotFoundException("Guardian not found");

        await _userService.UpdateAsync(guardian.UserId, request.User);

        guardian.DateOfBirth = request.DateOfBirth;
        _unitOfWork.Guardians.Update(guardian);
        await _unitOfWork.SaveChangesAsync();
    }
}