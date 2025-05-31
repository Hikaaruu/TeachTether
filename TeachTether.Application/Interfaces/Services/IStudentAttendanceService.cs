using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services;

public interface IStudentAttendanceService
{
    Task<StudentAttendanceResponse> GetByIdAsync(int id);
    Task<IEnumerable<StudentAttendanceResponse>> GetAllByStudentAsync(int studentId, int subjectId);
    Task<StudentAttendanceResponse> CreateAsync(CreateStudentAttendanceRequest request, int teacherId, int studentId);
    Task UpdateAsync(int id, UpdateStudentAttendanceRequest request);
    Task DeleteAsync(int id);
}