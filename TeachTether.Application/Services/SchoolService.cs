using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

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
            var school = _mapper.Map<School>(request);
            school.SchoolOwnerId = schoolOwnerId;

            await _unitOfWork.Schools.AddAsync(school);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SchoolResponse>(school);
        }

        public async Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<SchoolResponse>> GetAllByOwnerAsync(int schoolOwnerId)
        {
            var schools = await _unitOfWork.Schools.GetBySchoolOwnerIdAsync(schoolOwnerId);
            return _mapper.Map<IEnumerable<SchoolResponse>>(schools);
        }

        public async Task<SchoolResponse> GetByIdAsync(int id)
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(id) 
                ?? throw new NotFoundException("School not found");
            return _mapper.Map<SchoolResponse>(school);
        }

        public async Task UpdateAsync(int id, UpdateSchoolRequest request)
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(id)
                     ?? throw new NotFoundException("School not found");

            school.Name = request.Name;
            _unitOfWork.Schools.Update(school);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
