using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services;

public class StudentService(
    IUnitOfWork unitOfWork,
    IUserService userService,
    IMapper mapper,
    IStudentDeletionHelper studentDeletionHelper) : IStudentService
{
    private readonly IMapper _mapper = mapper;
    private readonly IStudentDeletionHelper _studentDeletionHelper = studentDeletionHelper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserService _userService = userService;

    public async Task<CreatedStudentResponse> CreateAsync(CreateStudentRequest request, int schoolId)
    {
        var (user, password) = await _userService.CreateAsync(request.User, UserType.Student);

        var student = new Student
        {
            UserId = user.Id!,
            DateOfBirth = request.DateOfBirth,
            SchoolId = schoolId
        };

        await _unitOfWork.Students.AddAsync(student);
        await _unitOfWork.SaveChangesAsync();

        return new CreatedStudentResponse
        {
            User = _mapper.Map<UserDto>(user),
            Id = student.Id,
            Username = user.UserName,
            Password = password,
            SchoolId = student.SchoolId,
            DateOfBirth = student.DateOfBirth
        };
    }

    public async Task DeleteAsync(int id)
    {
        await _studentDeletionHelper.DeleteStudentAsync(id);
    }

    public async Task<IEnumerable<StudentResponse>> GetAllByClassGroupAsync(int classGroupId)
    {
        var classGroupStudents = await _unitOfWork.ClassGroupStudents.GetByClassGroupIdAsync(classGroupId);

        var studentIds = classGroupStudents.Select(cgs => cgs.StudentId);
        var students = await _unitOfWork.Students.GetByIdsAsync(studentIds);

        var userIds = students.Select(s => s.UserId);
        var users = await _userService.GetByIdsAsync(userIds);
        var userMap = users
            .Where(u => u.Id != null)
            .ToDictionary(u => u.Id!);

        var result = new List<StudentResponse>();

        foreach (var student in students.Where(s => s != null))
        {
            if (!userMap.TryGetValue(student.UserId, out var user))
                throw new Exception($"User data can not be found for student with id = {student.Id}");

            var response = new StudentResponse
            {
                User = _mapper.Map<UserDto>(user),
                Id = student.Id,
                SchoolId = student.SchoolId,
                DateOfBirth = student.DateOfBirth
            };

            result.Add(response);
        }

        return result;
    }

    public async Task<IEnumerable<StudentResponse>> GetAllByGuardianAsync(int guardianId)
    {
        var studentIds = (await _unitOfWork.GuardianStudents
                .GetByGuardianIdAsync(guardianId))
            .Select(gs => gs.StudentId);

        var students = await _unitOfWork.Students.GetByIdsAsync(studentIds);

        var userIds = students.Select(s => s.UserId);
        var users = await _userService.GetByIdsAsync(userIds);
        var userMap = users
            .Where(u => u.Id != null)
            .ToDictionary(u => u.Id!);

        var result = new List<StudentResponse>();

        foreach (var student in students)
        {
            if (!userMap.TryGetValue(student.UserId, out var user))
                throw new Exception($"User data can not be found for student with id = {student.Id}");

            var response = new StudentResponse
            {
                User = _mapper.Map<UserDto>(user),
                Id = student.Id,
                SchoolId = student.SchoolId,
                DateOfBirth = student.DateOfBirth
            };

            result.Add(response);
        }

        return result;
    }

    public async Task<IEnumerable<StudentResponse>> GetAllBySchoolAsync(int schoolId)
    {
        var students = await _unitOfWork.Students.GetBySchoolIdAsync(schoolId);

        var userIds = students.Select(s => s.UserId);
        var users = await _userService.GetByIdsAsync(userIds);
        var userMap = users
            .Where(u => u.Id != null)
            .ToDictionary(u => u.Id!);

        var result = new List<StudentResponse>();

        foreach (var student in students)
        {
            if (!userMap.TryGetValue(student.UserId, out var user))
                throw new Exception($"User data can not be found for student with id = {student.Id}");

            var response = new StudentResponse
            {
                User = _mapper.Map<UserDto>(user),
                Id = student.Id,
                SchoolId = student.SchoolId,
                DateOfBirth = student.DateOfBirth
            };

            result.Add(response);
        }

        return result;
    }

    public async Task<StudentResponse> GetByIdAsync(int id)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(id)
                      ?? throw new NotFoundException("Student not found");

        var user = await _userService.GetByIdAsync(student.UserId);

        return new StudentResponse
        {
            User = _mapper.Map<UserDto>(user),
            Id = student.Id,
            SchoolId = student.SchoolId,
            DateOfBirth = student.DateOfBirth
        };
    }

    public async Task<IEnumerable<StudentResponse>> GetWithoutClassGroupAsync(int schoolId)
    {
        var allStudents = await _unitOfWork.Students.GetBySchoolIdAsync(schoolId);
        var studentIds = allStudents.Select(s => s.Id);

        var classGroupStudentMappings = await _unitOfWork.ClassGroupStudents
            .GetAllAsync(cgs => studentIds.Contains(cgs.StudentId));

        var assignedStudentIds = classGroupStudentMappings.Select(cgs => cgs.StudentId);

        var students = allStudents
            .Where(s => !assignedStudentIds.Contains(s.Id));

        var userIds = students.Select(s => s.UserId);
        var users = await _userService.GetByIdsAsync(userIds);
        var userMap = users
            .Where(u => u.Id != null)
            .ToDictionary(u => u.Id!);

        var result = new List<StudentResponse>();

        foreach (var student in students)
        {
            if (!userMap.TryGetValue(student.UserId, out var user))
                throw new Exception($"User data can not be found for student with id = {student.Id}");

            var response = new StudentResponse
            {
                User = _mapper.Map<UserDto>(user),
                Id = student.Id,
                SchoolId = student.SchoolId,
                DateOfBirth = student.DateOfBirth
            };

            result.Add(response);
        }

        return result;
    }

    public async Task UpdateAsync(int id, UpdateStudentRequest request)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(id)
                      ?? throw new NotFoundException("Student not found");

        await _userService.UpdateAsync(student.UserId, request.User);

        student.DateOfBirth = request.DateOfBirth;
        _unitOfWork.Students.Update(student);
        await _unitOfWork.SaveChangesAsync();
    }
}