using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.Application.Services
{
    public class StudentRecordsService : IStudentRecordsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudentRecordsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task DeleteForClassGroupSubject(int classGroupId, int subjectId)
        {
            var studentIds = (await _unitOfWork.ClassGroupStudents
                .GetByClassGroupIdAsync(classGroupId))
                .Select(cgs => cgs.StudentId);

            var grades = await _unitOfWork.StudentGrades.GetAllAsync(g => g.SubjectId == subjectId && studentIds.Contains(g.StudentId));
            _unitOfWork.StudentGrades.DeleteMany(grades);

            var behavior = await _unitOfWork.StudentBehaviors.GetAllAsync(g => g.SubjectId == subjectId && studentIds.Contains(g.StudentId));
            _unitOfWork.StudentBehaviors.DeleteMany(behavior);

            var att = await _unitOfWork.StudentAttendances.GetAllAsync(g => g.SubjectId == subjectId && studentIds.Contains(g.StudentId));
            _unitOfWork.StudentAttendances.DeleteMany(att);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
