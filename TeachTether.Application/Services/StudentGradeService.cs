using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services
{
    public class StudentGradeService : IStudentGradeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StudentGradeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<StudentGradeResponse> CreateAsync(CreateStudentGradeRequest request, int teacherId, int studentId)
        {
            var studentGrade = _mapper.Map<StudentGrade>(request);
            studentGrade.TeacherId = teacherId;
            studentGrade.StudentId = studentId;
            studentGrade.CreatedAt = DateTime.Now;

            await _unitOfWork.StudentGrades.AddAsync(studentGrade);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<StudentGradeResponse>(studentGrade);
        }

        public async Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<StudentGradeResponse>> GetAllByStudentAsync(int studentId)
        {
            var studentGrades = await _unitOfWork.StudentGrades.GetByStudentIdAsync(studentId);
            return _mapper.Map<IEnumerable<StudentGradeResponse>>(studentGrades);
        }

        public async Task<StudentGradeResponse> GetByIdAsync(int id)
        {
            var studentGrade = await _unitOfWork.StudentGrades.GetByIdAsync(id)
                ?? throw new NotFoundException("Student grade record not found");

            return _mapper.Map<StudentGradeResponse>(studentGrade);
        }

        public async Task UpdateAsync(int id, UpdateStudentGradeRequest request)
        {
            var studentGrade = await _unitOfWork.StudentGrades.GetByIdAsync(id)
                ?? throw new NotFoundException("Student grade record not found");

            _mapper.Map(request, studentGrade);

            _unitOfWork.StudentGrades.Update(studentGrade);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
