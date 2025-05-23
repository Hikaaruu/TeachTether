using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;

namespace TeachTether.Application.Services.DeletionHelpers
{
    public class TeacherDeletionHelper : ITeacherDeletionHelper
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IAnnouncementDeletionHelper _announcementDeletionHelper;
        private readonly IMessageThreadDeletionHelper _messageThreadDeletionHelper;
        private readonly IClassGroupDeletionHelper _classGroupDeletionHelper;

        public TeacherDeletionHelper(IUnitOfWork unitOfWork, IUserRepository userRepository, IAnnouncementDeletionHelper announcementDeletionHelper, IMessageThreadDeletionHelper messageThreadDeletionHelper, IClassGroupDeletionHelper classGroupDeletionHelper)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _announcementDeletionHelper = announcementDeletionHelper;
            _messageThreadDeletionHelper = messageThreadDeletionHelper;
            _classGroupDeletionHelper = classGroupDeletionHelper;
        }

        public async Task DeleteTeacherAsync(int id)
        {
            var teacher = await _unitOfWork.Teachers.GetByIdAsync(id)
                ?? throw new NotFoundException("Teacher not found");

            //WARNING
            var cg = await _unitOfWork.ClassGroups.GetByHomeroomTeacherIdAsync(id);
            if (cg.Any())
                throw new BadRequestException("Teacher is assigned to one of class groups as homeroom teacher");

            var grades = await _unitOfWork.StudentGrades.GetAllAsync(g => g.TeacherId == id);
            _unitOfWork.StudentGrades.DeleteMany(grades);
            var att = await _unitOfWork.StudentAttendances.GetAllAsync(g => g.TeacherId == id);
            _unitOfWork.StudentAttendances.DeleteMany(att);
            var beh = await _unitOfWork.StudentBehaviors.GetAllAsync(g => g.TeacherId == id);
            _unitOfWork.StudentBehaviors.DeleteMany(beh);

            var classAss = await _unitOfWork.ClassAssignments.GetByTeacherIdAsync(id);
            _unitOfWork.ClassAssignments.DeleteMany(classAss);

            var announcements = await _unitOfWork.Announcements.GetByTeacherIdAsync(id);
            foreach (var a in announcements)
            {
                await _announcementDeletionHelper.DeleteAnnouncementAsync(a.Id);
            }

            var threads = await _unitOfWork.MessageThreads.GetByTeacherIdAsync(id);
            foreach (var t in threads)
            {
                await _messageThreadDeletionHelper.DeleteMessageThreadAsync(t.Id);
            }

            _unitOfWork.Teachers.Delete(teacher);
            var result = await _userRepository.DeleteAsync(teacher.UserId);

            if (result.Succeeded)
            {
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                throw new Exception();
            }


        }
    }
}
