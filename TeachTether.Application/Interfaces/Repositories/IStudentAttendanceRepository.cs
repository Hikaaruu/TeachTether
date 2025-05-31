using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories;

public interface IStudentAttendanceRepository : IRepository<StudentAttendance>
{
    Task<IEnumerable<StudentAttendance>> GetByStudentIdAsync(int studentId);
    Task<IEnumerable<StudentAttendance>> GetByStudentAndSubjectIdAsync(int studentId, int subjectId);
}