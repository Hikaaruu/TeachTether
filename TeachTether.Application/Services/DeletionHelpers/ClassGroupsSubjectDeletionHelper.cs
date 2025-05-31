using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;

namespace TeachTether.Application.Services.DeletionHelpers
{
    public class ClassGroupsSubjectDeletionHelper : IClassGroupsSubjectDeletionHelper
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClassGroupsSubjectDeletionHelper(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task DeleteClassGroupsSubjectAsync(int id)
        {
            var classGroupSubject = await _unitOfWork.ClassGroupsSubjects.GetByIdAsync(id)
                ?? throw new NotFoundException("Class Group Subject Not Found");

            var classGroupId = classGroupSubject.ClassGroupId;
            var subjectId = classGroupSubject.SubjectId;

            var studentIds = (await _unitOfWork.ClassGroupStudents
                .GetByClassGroupIdAsync(classGroupId))
                .Select(cgs => cgs.StudentId);

            var grades = await _unitOfWork.StudentGrades.GetAllAsync(g => g.SubjectId == subjectId && studentIds.Contains(g.StudentId));
            _unitOfWork.StudentGrades.DeleteMany(grades);

            var behavior = await _unitOfWork.StudentBehaviors.GetAllAsync(g => g.SubjectId == subjectId && studentIds.Contains(g.StudentId));
            _unitOfWork.StudentBehaviors.DeleteMany(behavior);

            var att = await _unitOfWork.StudentAttendances.GetAllAsync(g => g.SubjectId == subjectId && studentIds.Contains(g.StudentId));
            _unitOfWork.StudentAttendances.DeleteMany(att);


            var classAssignments = await _unitOfWork.ClassAssignments.GetByClassGroupSubjectIdAsync(id);
            _unitOfWork.ClassAssignments.DeleteMany(classAssignments);

            _unitOfWork.ClassGroupsSubjects.Delete(classGroupSubject);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
