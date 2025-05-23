using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;

namespace TeachTether.Application.Services.DeletionHelpers
{
    public class ClassGroupDeletionHelper : IClassGroupDeletionHelper
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClassGroupsSubjectDeletionHelper _classGroupsSubjectDeletionHelper;

        public ClassGroupDeletionHelper(IUnitOfWork unitOfWork, IClassGroupsSubjectDeletionHelper subjectDeletionHelper)
        {
            _unitOfWork = unitOfWork;
            _classGroupsSubjectDeletionHelper = subjectDeletionHelper;
        }

        public async Task DeleteClassGroupAsync(int id)
        {
            var classGroup = await _unitOfWork.ClassGroups.GetByIdAsync(id)
                ?? throw new NotFoundException("Class group not found");

            var cgSubjects = await _unitOfWork.ClassGroupsSubjects.GetByClassGroupIdAsync(id);
            foreach (var cgs in cgSubjects)
            {
                await _classGroupsSubjectDeletionHelper.DeleteClassGroupsSubjectAsync(cgs.Id);
            }

            var cgStudents = await _unitOfWork.ClassGroupStudents.GetByClassGroupIdAsync(id);
            _unitOfWork.ClassGroupStudents.DeleteMany(cgStudents);

            var acg = await _unitOfWork.AnnouncementClassGroups.GetByClassGroupIdAsync(id);
            _unitOfWork.AnnouncementClassGroups.DeleteMany(acg);

            _unitOfWork.ClassGroups.Delete(classGroup);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
