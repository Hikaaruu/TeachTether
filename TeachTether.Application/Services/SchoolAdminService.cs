using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services
{
    public class SchoolAdminService : ISchoolAdminService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly ISchoolAdminDeletionHelper _schoolAdminDeletionHelper;

        public SchoolAdminService(IMapper mapper, IUnitOfWork unitOfWork, IUserService userService, ISchoolAdminDeletionHelper schoolAdminDeletionHelper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userService = userService;
            _schoolAdminDeletionHelper = schoolAdminDeletionHelper;
        }

        public async Task<IEnumerable<SchoolAdminResponse>> GetAllBySchoolAsync(int schoolId)
        {
            var schoolAdmins = await _unitOfWork.SchoolAdmins.GetBySchoolIdAsync(schoolId);

            var userIds = schoolAdmins.Select(sa => sa.UserId);
            var users = await _userService.GetByIdsAsync(userIds);
            var userMap = users
                .Where(u => u.Id != null)
                .ToDictionary(u => u.Id!);

            var responses = new List<SchoolAdminResponse>();

            foreach (var admin in schoolAdmins)
            {
                if (!userMap.TryGetValue(admin.UserId, out var user))
                    throw new Exception($"User data can not be found for school admin with id = {admin.Id}");

                var response = new SchoolAdminResponse
                {
                    Id = admin.Id,
                    SchoolId = admin.SchoolId,
                    User = _mapper.Map<UserDto>(user)
                };

                responses.Add(response);
            }

            return responses;
        }

        public async Task<SchoolAdminResponse> GetByIdAsync(int id)
        {
            var schoolAdmin = await _unitOfWork.SchoolAdmins.GetByIdAsync(id)
                ?? throw new NotFoundException("School admin not found");

            var user = await _userService.GetByIdAsync(schoolAdmin.UserId);

            return new SchoolAdminResponse()
            {
                User = _mapper.Map<UserDto>(user),
                Id = schoolAdmin.Id,
                SchoolId = schoolAdmin.SchoolId
            };
        }

        public async Task<CreatedSchoolAdminResponse> CreateAsync(CreateSchoolAdminRequest request, int schoolId)
        {
            (var user, var password) = await _userService.CreateAsync(request.User, UserType.SchoolAdmin);

            var schoolAdmin = new SchoolAdmin
            {
                UserId = user.Id!,
                SchoolId = schoolId
            };

            await _unitOfWork.SchoolAdmins.AddAsync(schoolAdmin);
            await _unitOfWork.SaveChangesAsync();


            return new CreatedSchoolAdminResponse
            {
                User = _mapper.Map<UserDto>(user),
                Username = user.UserName,
                Password = password,
                SchoolId = schoolAdmin.SchoolId,
                Id = schoolAdmin.Id
            };
        }

        public async Task UpdateAsync(int id, UpdateSchoolAdminRequest request)
        {
            var schoolAdmin = await _unitOfWork.SchoolAdmins.GetByIdAsync(id)
                ?? throw new NotFoundException("School admin not found");

            await _userService.UpdateAsync(schoolAdmin.UserId, request.User);
        }

        public async Task DeleteAsync(int id)
        {
            await _schoolAdminDeletionHelper.DeleteSchoolAdminAsync(id);
        }
    }
}
