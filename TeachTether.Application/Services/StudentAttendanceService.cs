using AutoMapper;
using System.Reflection.Metadata.Ecma335;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services
{
    public class StudentAttendanceService : IStudentAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StudentAttendanceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<StudentAttendanceResponse> CreateAsync(CreateStudentAttendanceRequest request, int teacherId, int studentId)
        {
            var studentAttendance = _mapper.Map<StudentAttendance>(request);
            studentAttendance.TeacherId = teacherId;
            studentAttendance.StudentId = studentId;
            studentAttendance.CreatedAt = DateTime.Now;

            await _unitOfWork.StudentAttendances.AddAsync(studentAttendance);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<StudentAttendanceResponse>(studentAttendance);
        }

        public async Task DeleteAsync(int id)
        {
            var att = await _unitOfWork.StudentAttendances.GetByIdAsync(id)
                ?? throw new NotFoundException("Attendance Record not found");
            _unitOfWork.StudentAttendances.Delete(att);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<StudentAttendanceResponse>> GetAllByStudentAsync(int studentId, int subjectId)
        {
            var studentAttendances = await _unitOfWork.StudentAttendances.GetAllAsync(sg => sg.StudentId == studentId && sg.SubjectId == subjectId);
            return _mapper.Map<IEnumerable<StudentAttendanceResponse>>(studentAttendances);
        }

        public async Task<StudentAttendanceResponse> GetByIdAsync(int id)
        {
            var studentAttendance = await _unitOfWork.StudentAttendances.GetByIdAsync(id)
                ?? throw new NotFoundException("Student attendances record not found");

            return _mapper.Map<StudentAttendanceResponse>(studentAttendance);
        }

        public async Task UpdateAsync(int id, UpdateStudentAttendanceRequest request)
        {
            var studentAttendance = await _unitOfWork.StudentAttendances.GetByIdAsync(id)
                ?? throw new NotFoundException("Student attendances record not found");

            _mapper.Map(request, studentAttendance);

            _unitOfWork.StudentAttendances.Update(studentAttendance);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
