using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;

namespace TeachTether.Application.Services.DeletionHelpers
{
    public class SubjectDeletionHelper : ISubjectDeletionHelper
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClassGroupsSubjectDeletionHelper _classGroupsSubjectDeletion;

        public SubjectDeletionHelper(IUnitOfWork unitOfWork, IClassGroupsSubjectDeletionHelper classGroupsSubjectDeletionHelper)
        {
            _unitOfWork = unitOfWork;
            _classGroupsSubjectDeletion = classGroupsSubjectDeletionHelper;
        }

        public async Task DeleteSubjectAsync(int id)
        {
            var subject = await _unitOfWork.Subjects.GetByIdAsync(id)
                ?? throw new NotFoundException("Subject Not Found");
            var classGroupSubjects = await _unitOfWork.ClassGroupsSubjects.GetBySubjectIdAsync(id);
            foreach (var cgs in classGroupSubjects)
            {
                await _classGroupsSubjectDeletion.DeleteClassGroupsSubjectAsync(cgs.Id);
            }

            var grades = await _unitOfWork.StudentGrades.GetAllAsync(g => g.SubjectId == id);
            _unitOfWork.StudentGrades.DeleteMany(grades);

            var behavior = await _unitOfWork.StudentBehaviors.GetAllAsync(g => g.SubjectId == id);
            _unitOfWork.StudentBehaviors.DeleteMany(behavior);

            var att = await _unitOfWork.StudentAttendances.GetAllAsync(g => g.SubjectId == id);
            _unitOfWork.StudentAttendances.DeleteMany(att);

            _unitOfWork.Subjects.Delete(subject);
            await _unitOfWork.SaveChangesAsync();

        }
    }
}
