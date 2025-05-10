using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Services
{
    public class StudentBehaviorService : IStudentBehaviorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StudentBehaviorService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<StudentBehaviorResponse> CreateAsync(CreateStudentBehaviorRequest request, int teacherId, int studentId)
        {
            var studentBehavior = _mapper.Map<StudentBehavior>(request);
            studentBehavior.TeacherId = teacherId;
            studentBehavior.StudentId = studentId;
            studentBehavior.CreatedAt = DateTime.Now;

            await _unitOfWork.StudentBehaviors.AddAsync(studentBehavior);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<StudentBehaviorResponse>(studentBehavior);
        }

        public async Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<StudentBehaviorResponse>> GetAllByStudentAsync(int studentId)
        {
            var studentBehaviors = await _unitOfWork.StudentBehaviors.GetByStudentIdAsync(studentId);
            return _mapper.Map<IEnumerable<StudentBehaviorResponse>>(studentBehaviors);
        }

        public async Task<StudentBehaviorResponse> GetByIdAsync(int id)
        {
            var studentBehavior = await _unitOfWork.StudentBehaviors.GetByIdAsync(id)
                ?? throw new NotFoundException("Student behavior record not found");

            return _mapper.Map<StudentBehaviorResponse>(studentBehavior);
        }

        public async Task UpdateAsync(int id, UpdateStudentBehaviorRequest request)
        {
            var studentBehavior = await _unitOfWork.StudentBehaviors.GetByIdAsync(id)
                ?? throw new NotFoundException("Student behavior record not found");

            _mapper.Map(request, studentBehavior);

            _unitOfWork.StudentBehaviors.Update(studentBehavior);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
