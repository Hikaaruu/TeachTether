using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SubjectService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<SubjectResponse> CreateAsync(CreateSubjectRequest request, int schoolId)
        {
            if (await _unitOfWork.Subjects.AnyAsync(s =>
                s.SchoolId == schoolId &&
                s.Name == request.Name))
            {
                throw new BadRequestException($"Subject \"{request.Name}\" already exists in school {schoolId}.");
            }

            var subject = _mapper.Map<Subject>(request);
            subject.SchoolId = schoolId;

            await _unitOfWork.Subjects.AddAsync(subject);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SubjectResponse>(subject);
        }

        public async Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<SubjectResponse>> GetAllByClassGroupAsync(int classGroupId)
        {
            var subjectIds = (await _unitOfWork.ClassGroupsSubjects
                .GetByClassGroupIdAsync(classGroupId))
                .Select(cgs => cgs.SubjectId);

            var subjects = await _unitOfWork.Subjects.GetByIdsAsync(subjectIds);
            return _mapper.Map<IEnumerable<SubjectResponse>>(subjects);
        }

        public async Task<IEnumerable<SubjectResponse>> GetAllBySchoolAsync(int schoolId)
        {
            var subjects = await _unitOfWork.Subjects.GetBySchoolIdAsync(schoolId);
            return _mapper.Map<IEnumerable<SubjectResponse>>(subjects);
        }

        public async Task<SubjectResponse> GetByIdAsync(int id)
        {
            var subject = await _unitOfWork.Subjects.GetByIdAsync(id)
                ?? throw new NotFoundException("Subject not found");

            return _mapper.Map<SubjectResponse>(subject);
        }

        public async Task UpdateAsync(int id, UpdateSubjectRequest request)
        {
            var subject = await _unitOfWork.Subjects.GetByIdAsync(id)
                ?? throw new NotFoundException("Subject not found");

            if (await _unitOfWork.Subjects.AnyAsync(s =>
                s.SchoolId == subject.SchoolId &&
                s.Name == request.Name))
            {
                throw new BadRequestException($"Subject \"{request.Name}\" already exists in school {subject.SchoolId}.");
            }

            _mapper.Map(request, subject);
            _unitOfWork.Subjects.Update(subject);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
