using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public AnnouncementService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<AnnouncementResponse> CreateAsync(CreateAnnouncementRequest request, int teacherId)
        {
            var announcement = _mapper.Map<Announcement>(request);
            announcement.TeacherId = teacherId;
            announcement.CreatedAt = DateTime.Now;

            await _unitOfWork.Announcements.AddAsync(announcement);

            foreach (var item in request.ClassGroupIds)
            {
                var announcementClassGroup = new AnnouncementClassGroup()
                {
                    AnnouncementId = announcement.Id,
                    ClassGroupId = item
                };
                await _unitOfWork.AnnouncementClassGroups.AddAsync(announcementClassGroup);
            }
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<AnnouncementResponse>(announcement);
        }

        public async Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<AnnouncementResponse>> GetAllForUserAsync(string userId)
        {
            var user = await _userService.GetByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            IEnumerable<Announcement> announcements;

            switch (user.UserType)
            {
                case UserType.Teacher:
                    {
                        var teacher = await _unitOfWork.Teachers.GetByUserIdAsync(userId)
                            ?? throw new NotFoundException("Teacher not found");

                        announcements = await _unitOfWork.Announcements.GetByTeacherIdAsync(teacher.Id);
                        break;
                    }

                case UserType.Guardian:
                    {
                        var guardian = await _unitOfWork.Guardians.GetByUserIdAsync(userId)
                            ?? throw new NotFoundException("Guardian not found");

                        announcements = await _unitOfWork.Announcements.GetAllAsync((a => a.TargetAudience == AudienceType.Guardian || a.TargetAudience == AudienceType.StudentAndGuardian));
                        break;
                    }

                default:
                    throw new Exception("Unexpected behavior occurred");
            }


            return _mapper.Map<IEnumerable<MessageThreadResponse>>(threads);
        }

        public async Task<AnnouncementResponse> GetByIdAsync(int id)
        {
            var announcement = await _unitOfWork.Announcements.GetByIdAsync(id)
                ?? throw new NotFoundException("Announcement not found");

            return _mapper.Map<AnnouncementResponse>(announcement);
        }

        public async Task UpdateAsync(int id, UpdateAnnouncementRequest request)
        {
            var announcement = await _unitOfWork.Announcements.GetByIdAsync(id)
                ?? throw new NotFoundException("Announcement not found");

            _mapper.Map(request, announcement);
            _unitOfWork.Announcements.Update(announcement);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
