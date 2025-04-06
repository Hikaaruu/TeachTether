using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.Application.Services
{
    public class SchoolService : ISchoolService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SchoolService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<SchoolResponse> CreateAsync(CreateSchoolRequest request, int schoolOwnerId)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<SchoolResponse>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<SchoolResponse> GetByIdAsync(int id)
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(id) 
                ?? throw new NotFoundException("School not found");
            return _mapper.Map<SchoolResponse>(school);
        }

        public async Task UpdateAsync(int id, UpdateSchoolRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
