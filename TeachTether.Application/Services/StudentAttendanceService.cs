using AutoMapper;
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
        private readonly IUserService _userService;

        public StudentAttendanceService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<StudentAttendanceResponse> CreateAsync(CreateStudentAttendanceRequest request, int teacherId, int studentId)
        {
            var studentAttendance = _mapper.Map<StudentAttendance>(request);
            studentAttendance.TeacherId = teacherId;
            studentAttendance.StudentId = studentId;
            studentAttendance.CreatedAt = DateTime.Now;

            await _unitOfWork.StudentAttendances.AddAsync(studentAttendance);
            await _unitOfWork.SaveChangesAsync();

            var response =  _mapper.Map<StudentAttendanceResponse>(studentAttendance);

            var teacher = await _unitOfWork.Teachers.GetByIdAsync(studentAttendance.TeacherId);
            var user = await _userService.GetByIdAsync(teacher!.UserId);
            response.TeacherName = BuildFullName(user);

            return response;
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
            var studentAttendances = await _unitOfWork.StudentAttendances
                .GetAllAsync(sg => sg.StudentId == studentId && sg.SubjectId == subjectId);

            var teachers = await _unitOfWork.Teachers.GetByIdsAsync(studentAttendances.Select(sg => sg.TeacherId).Distinct());

            var userIds = teachers
                .Select(t => t.UserId)
                .Distinct();

            var users = await _userService.GetByIdsAsync(userIds);

            var teacherToUser = teachers.ToDictionary(t => t.Id, t => t.UserId);

            var userIdToName = users.ToDictionary(
                u => u.Id!,
                BuildFullName
            );

            var responses =  _mapper.Map<IEnumerable<StudentAttendanceResponse>>(studentAttendances);

            foreach (var resp in responses)
            {
                if (teacherToUser.TryGetValue(resp.TeacherId, out var userId) &&
                    userId != null &&
                    userIdToName.TryGetValue(userId, out var fullName))
                {
                    resp.TeacherName = fullName;
                }
                else
                {
                    throw new Exception();
                }
            }

            return responses;
        }

        public async Task<StudentAttendanceResponse> GetByIdAsync(int id)
        {
            var studentAttendance = await _unitOfWork.StudentAttendances.GetByIdAsync(id)
                ?? throw new NotFoundException("Student attendances record not found");

            var response = _mapper.Map<StudentAttendanceResponse>(studentAttendance);

            var teacher = await _unitOfWork.Teachers.GetByIdAsync(studentAttendance.TeacherId);
            var user = await _userService.GetByIdAsync(teacher!.UserId);
            response.TeacherName = BuildFullName(user);

            return response;
        }

        public async Task UpdateAsync(int id, UpdateStudentAttendanceRequest request)
        {
            var studentAttendance = await _unitOfWork.StudentAttendances.GetByIdAsync(id)
                ?? throw new NotFoundException("Student attendances record not found");

            _mapper.Map(request, studentAttendance);

            _unitOfWork.StudentAttendances.Update(studentAttendance);
            await _unitOfWork.SaveChangesAsync();
        }

        private static string BuildFullName(User user)
        {
            return string.Join(" ",
                new[] { user.FirstName, user.MiddleName, user.LastName }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
        }
    }
}
