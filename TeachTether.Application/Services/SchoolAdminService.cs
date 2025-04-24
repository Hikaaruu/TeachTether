using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Common.Interfaces;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services
{
    public class SchoolAdminService : ISchoolAdminService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly ICredentialsGenerator _credentialsGenerator;
        private readonly IUserCreationService _userCreationService;

        public SchoolAdminService(IMapper mapper, IUnitOfWork unitOfWork, IUserRepository userRepository, ICredentialsGenerator credentialsGenerator, IUserCreationService userCreationService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _credentialsGenerator = credentialsGenerator;
            _userCreationService = userCreationService;
        }

        public async Task<IEnumerable<SchoolAdminResponse>> GetAllBySchoolAsync(int schoolId)
        {
            var schoolAdmins = await _unitOfWork.SchoolAdmins.GetBySchoolIdAsync(schoolId);
            return _mapper.Map<IEnumerable<SchoolAdminResponse>>(schoolAdmins);
        }

        public async Task<SchoolAdminResponse> GetByIdAsync(int id)
        {
            var schoolAdmin = await _unitOfWork.SchoolAdmins.GetByIdAsync(id)
                ?? throw new NotFoundException("School admin not found");

            return _mapper.Map<SchoolAdminResponse>(schoolAdmin);
        }

        public async Task<CreatedSchoolAdminResponse> CreateAsync(CreateSchoolAdminRequest request, int schoolId)
        {
            (var user, var password) = await _userCreationService
                .CreateAsync(request.User,UserType.SchoolAdmin);

            var schoolAdmin = new SchoolAdmin
            {
                UserId = user.Id!,
                SchoolId = schoolId
            };

            await _unitOfWork.SchoolAdmins.AddAsync(schoolAdmin);
            await _unitOfWork.SaveChangesAsync();

            var response = _mapper.Map<CreatedSchoolAdminResponse>(user);
            response.Password = password;
            response.SchoolId = schoolId;

            return response;
        }

        public  async Task UpdateAsync(int id, UpdateSchoolAdminRequest request)
        {
            var schoolAdmin = await _unitOfWork.SchoolAdmins.GetByIdAsync(id)
                ?? throw new NotFoundException("School admin not found");

            var result = await _userRepository.UpdateAsync(schoolAdmin.UserId,request.User);

            if (!result.Succeeded)
                throw new Exception("Failed to update user");
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
