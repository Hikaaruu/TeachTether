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

            var classAssignments = await _unitOfWork.ClassAssignments.GetByClassGroupSubjectIdAsync(id);
            _unitOfWork.ClassAssignments.DeleteMany(classAssignments);
            _unitOfWork.ClassGroupsSubjects.Delete(classGroupSubject);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
