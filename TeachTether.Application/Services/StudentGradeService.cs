using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services;

public class StudentGradeService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService)
    : IStudentGradeService
{
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserService _userService = userService;

    public async Task<StudentGradeResponse> CreateAsync(CreateStudentGradeRequest request, int teacherId, int studentId)
    {
        var studentGrade = _mapper.Map<StudentGrade>(request);
        studentGrade.TeacherId = teacherId;
        studentGrade.StudentId = studentId;
        studentGrade.CreatedAt = DateTime.Now;

        await _unitOfWork.StudentGrades.AddAsync(studentGrade);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<StudentGradeResponse>(studentGrade);

        var teacher = await _unitOfWork.Teachers.GetByIdAsync(studentGrade.TeacherId);
        var user = await _userService.GetByIdAsync(teacher!.UserId);
        response.TeacherName = BuildFullName(user);

        return response;
    }

    public async Task DeleteAsync(int id)
    {
        var grade = await _unitOfWork.StudentGrades.GetByIdAsync(id)
                    ?? throw new NotFoundException("Grade Record not found");
        _unitOfWork.StudentGrades.Delete(grade);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<StudentGradeResponse>> GetAllByStudentAsync(int studentId, int subjectId)
    {
        var studentGrades = await _unitOfWork.StudentGrades
            .GetAllAsync(sg => sg.StudentId == studentId && sg.SubjectId == subjectId);

        var teachers = await _unitOfWork.Teachers.GetByIdsAsync(studentGrades.Select(sg => sg.TeacherId).Distinct());

        var userIds = teachers
            .Select(t => t.UserId)
            .Distinct();

        var users = await _userService.GetByIdsAsync(userIds);

        var teacherToUser = teachers.ToDictionary(t => t.Id, t => t.UserId);

        var userIdToName = users.ToDictionary(
            u => u.Id!,
            BuildFullName
        );

        var responses = _mapper.Map<IEnumerable<StudentGradeResponse>>(studentGrades);

        foreach (var resp in responses)
            if (teacherToUser.TryGetValue(resp.TeacherId, out var userId) &&
                userId != null &&
                userIdToName.TryGetValue(userId, out var fullName))
                resp.TeacherName = fullName;
            else
                throw new Exception();

        return responses;
    }

    public async Task<StudentGradeResponse> GetByIdAsync(int id)
    {
        var studentGrade = await _unitOfWork.StudentGrades.GetByIdAsync(id)
                           ?? throw new NotFoundException("Student grade record not found");

        var response = _mapper.Map<StudentGradeResponse>(studentGrade);

        var teacher = await _unitOfWork.Teachers.GetByIdAsync(studentGrade.TeacherId);
        var user = await _userService.GetByIdAsync(teacher!.UserId);
        response.TeacherName = BuildFullName(user);

        return response;
    }

    public async Task UpdateAsync(int id, UpdateStudentGradeRequest request)
    {
        var studentGrade = await _unitOfWork.StudentGrades.GetByIdAsync(id)
                           ?? throw new NotFoundException("Student grade record not found");

        _mapper.Map(request, studentGrade);

        _unitOfWork.StudentGrades.Update(studentGrade);
        await _unitOfWork.SaveChangesAsync();
    }

    private static string BuildFullName(User user)
    {
        return string.Join(" ",
            new[] { user.FirstName, user.MiddleName, user.LastName }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}