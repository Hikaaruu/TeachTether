using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services
{
    public class ClassGroupService : IClassGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ClassGroupService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ClassGroupResponse> CreateAsync(CreateClassGroupRequest request, int schoolId)
        {
            var teacher = await _unitOfWork.Teachers.GetByIdAsync(request.HomeroomTeacherId);

            if (teacher is null || teacher.SchoolId != schoolId)
                throw new NotFoundException("Teacher not found");

            if (await _unitOfWork.ClassGroups.AnyAsync(cg => 
                cg.SchoolId == schoolId && 
                cg.GradeYear == request.GradeYear && 
                cg.Section == request.Section))
            {
                throw new BadRequestException($"Class group {request.GradeYear} - {request.Section} already exists in school {schoolId}.");
            }

            var classGroup = _mapper.Map<ClassGroup>(request);
            classGroup.SchoolId = schoolId;

            await _unitOfWork.ClassGroups.AddAsync(classGroup);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ClassGroupResponse>(classGroup);
        }

        public async Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ClassGroupResponse>> GetAllBySchoolAsync(int schoolId)
        {
            var classGroups = await _unitOfWork.ClassGroups.GetBySchoolIdAsync(schoolId);
            return _mapper.Map<IEnumerable<ClassGroupResponse>>(classGroups);
        }

        public async Task<ClassGroupResponse> GetByIdAsync(int id)
        {
            var classGroup = await _unitOfWork.ClassGroups.GetByIdAsync(id)
                ?? throw new NotFoundException("Class Group not found");

            return _mapper.Map<ClassGroupResponse>(classGroup);
        }

        public async Task UpdateAsync(int id, UpdateClassGroupRequest request)
        {
            var classGroup = await _unitOfWork.ClassGroups.GetByIdAsync(id)
                 ?? throw new NotFoundException("Class Group not found");

            if (await _unitOfWork.ClassGroups.AnyAsync(cg =>
                cg.SchoolId == classGroup.SchoolId &&
                cg.GradeYear == request.GradeYear &&
                cg.Section == request.Section))
            {
                throw new BadRequestException($"Class group {request.GradeYear} - {request.Section} already exists in school {classGroup.SchoolId}.");
            }

            _mapper.Map(request, classGroup);
            _unitOfWork.ClassGroups.Update(classGroup);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
