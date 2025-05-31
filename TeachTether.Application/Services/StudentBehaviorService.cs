using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services;

public class StudentBehaviorService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService)
    : IStudentBehaviorService
{
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserService _userService = userService;

    public async Task<StudentBehaviorResponse> CreateAsync(CreateStudentBehaviorRequest request, int teacherId,
        int studentId)
    {
        var studentBehavior = _mapper.Map<StudentBehavior>(request);
        studentBehavior.TeacherId = teacherId;
        studentBehavior.StudentId = studentId;
        studentBehavior.CreatedAt = DateTime.Now;

        await _unitOfWork.StudentBehaviors.AddAsync(studentBehavior);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<StudentBehaviorResponse>(studentBehavior);

        var teacher = await _unitOfWork.Teachers.GetByIdAsync(studentBehavior.TeacherId);
        var user = await _userService.GetByIdAsync(teacher!.UserId);
        response.TeacherName = BuildFullName(user);

        return response;
    }

    public async Task DeleteAsync(int id)
    {
        var behavior = await _unitOfWork.StudentBehaviors.GetByIdAsync(id)
                       ?? throw new NotFoundException("Behavior Record not found");
        _unitOfWork.StudentBehaviors.Delete(behavior);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<StudentBehaviorResponse>> GetAllByStudentAsync(int studentId, int subjectId)
    {
        var studentBehaviors =
            await _unitOfWork.StudentBehaviors.GetAllAsync(sg =>
                sg.StudentId == studentId && sg.SubjectId == subjectId);

        var teachers = await _unitOfWork.Teachers.GetByIdsAsync(studentBehaviors.Select(sg => sg.TeacherId).Distinct());

        var userIds = teachers
            .Select(t => t.UserId)
            .Distinct();

        var users = await _userService.GetByIdsAsync(userIds);

        var teacherToUser = teachers.ToDictionary(t => t.Id, t => t.UserId);

        var userIdToName = users.ToDictionary(
            u => u.Id!,
            BuildFullName
        );

        var responses = _mapper.Map<IEnumerable<StudentBehaviorResponse>>(studentBehaviors);

        foreach (var resp in responses)
            if (teacherToUser.TryGetValue(resp.TeacherId, out var userId) &&
                userId != null &&
                userIdToName.TryGetValue(userId, out var fullName))
                resp.TeacherName = fullName;
            else
                throw new Exception();

        return responses;
    }

    public async Task<StudentBehaviorResponse> GetByIdAsync(int id)
    {
        var studentBehavior = await _unitOfWork.StudentBehaviors.GetByIdAsync(id)
                              ?? throw new NotFoundException("Student behavior record not found");

        var response = _mapper.Map<StudentBehaviorResponse>(studentBehavior);

        var teacher = await _unitOfWork.Teachers.GetByIdAsync(studentBehavior.TeacherId);
        var user = await _userService.GetByIdAsync(teacher!.UserId);
        response.TeacherName = BuildFullName(user);

        return response;
    }

    public async Task UpdateAsync(int id, UpdateStudentBehaviorRequest request)
    {
        var studentBehavior = await _unitOfWork.StudentBehaviors.GetByIdAsync(id)
                              ?? throw new NotFoundException("Student behavior record not found");

        _mapper.Map(request, studentBehavior);

        _unitOfWork.StudentBehaviors.Update(studentBehavior);
        await _unitOfWork.SaveChangesAsync();
    }

    private static string BuildFullName(User user)
    {
        return string.Join(" ",
            new[] { user.FirstName, user.MiddleName, user.LastName }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}