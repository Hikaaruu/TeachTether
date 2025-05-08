using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services
{
    public class ClassGroupSubjectService : IClassGroupSubjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ClassGroupSubjectService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task CreateAsync(CreateClassGroupSubjectRequest request, int classGroupId)
        {
            if (await _unitOfWork.ClassGroupsSubjects.AnyAsync(cgs =>
            cgs.ClassGroupId == classGroupId &&
            cgs.SubjectId == request.SubjectId))
            {
                throw new BadRequestException($"Subject {request.SubjectId} is already assigned to class group {classGroupId}.");
            }

            var classGroupSubject = _mapper.Map<ClassGroupSubject>(request);
            classGroupSubject.ClassGroupId = classGroupId;

            await _unitOfWork.ClassGroupsSubjects.AddAsync(classGroupSubject);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int classGroupId, int subjectId)
        {
            var classGroupSubject = (await _unitOfWork.ClassGroupsSubjects
                .GetByClassGroupIdAsync(classGroupId))
                .SingleOrDefault(cgs => cgs.SubjectId == subjectId)            
                ?? throw new NotFoundException("Subject is not assigned to this class group");

            _unitOfWork.ClassGroupsSubjects.Delete(classGroupSubject);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
