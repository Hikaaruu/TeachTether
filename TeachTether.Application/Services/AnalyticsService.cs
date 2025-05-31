using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services;

public class AnalyticsService(IUnitOfWork unitOfWork, IMapper mapper) : IAnalyticsService
{
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ClassAveragesResponse> GetClassAverages(int subjectId, int studentId, int classGroupId)
    {
        var classGroupStudent = await _unitOfWork.ClassGroupStudents.GetByStudentIdAsync(studentId);

        if (classGroupStudent is null || classGroupStudent.ClassGroupId != classGroupId)
            throw new BadRequestException("student is not in a class group specified");

        var studentIds = (await _unitOfWork.ClassGroupStudents
                .GetAllAsync(cgs => cgs.ClassGroupId == classGroupId))
            .Select(cgs => cgs.StudentId)
            .Where(id => id != studentId);


        var grades = await _unitOfWork.StudentGrades
            .GetAllAsync(sg => studentIds.Contains(sg.StudentId) &&
                               sg.SubjectId == subjectId);

        var behaviors = await _unitOfWork.StudentBehaviors
            .GetAllAsync(sb => studentIds.Contains(sb.StudentId) &&
                               sb.SubjectId == subjectId);

        var attendances = await _unitOfWork.StudentAttendances
            .GetAllAsync(sa => studentIds.Contains(sa.StudentId) &&
                               sa.SubjectId == subjectId);

        var gradeAverage = grades.Any() ? grades.Average(g => g.GradeValue) : (decimal?)null;
        var behaviorAverage = behaviors.Any() ? behaviors.Average(b => b.BehaviorScore) : (decimal?)null;

        var totalAttendance = attendances.Count();
        var attendance = new AttendanceBreakdown();

        if (totalAttendance > 0)
        {
            attendance.PresentPercentage =
                attendances.Count(a => a.Status == AttendanceStatus.Present) * 100m / totalAttendance;
            attendance.LatePercentage =
                attendances.Count(a => a.Status == AttendanceStatus.Late) * 100m / totalAttendance;
            attendance.AbsentPercentage =
                attendances.Count(a => a.Status == AttendanceStatus.Absent) * 100m / totalAttendance;
            attendance.ExcusedPercentage =
                attendances.Count(a => a.Status == AttendanceStatus.Excused) * 100m / totalAttendance;
        }

        return new ClassAveragesResponse
        {
            GradeAverage = gradeAverage,
            BehaviorAverage = behaviorAverage,
            Attendance = attendance
        };
    }
}