using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public StudentService(IUnitOfWork unitOfWork, IUserService userService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<CreatedStudentResponse> CreateAsync(CreateStudentRequest request, int schoolId)
        {
            (var user, var password) = await _userService.CreateAsync(request.User, UserType.Student);

            var student = new Student
            {
                UserId = user.Id!,
                DateOfBirth = request.DateOfBirth,
                SchoolId = schoolId
            };

            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.SaveChangesAsync();

            return new CreatedStudentResponse
            {
                User = _mapper.Map<UserDto>(user),
                Id = student.Id,
                Username = user.UserName,
                Password = password,
                SchoolId = student.SchoolId,
                DateOfBirth = student.DateOfBirth
            };
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<StudentResponse>> GetAllByClassGroupAsync(int classGroupId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<StudentResponse>> GetAllBySchoolAsync(int schoolId)
        {
            throw new NotImplementedException();
        }

        public async Task<StudentResponse> GetByIdAsync(int id)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(id)
                ?? throw new NotFoundException("Student not found");

            var user = _userService.GetByIdAsync(student.UserId);

            return new StudentResponse()
            {
                User = _mapper.Map<UserDto>(user),
                Id = student.Id,
                SchoolId=student.SchoolId,
                DateOfBirth = student.DateOfBirth
            };
        }

        public async Task UpdateAsync(int id, UpdateStudentRequest request)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(id)
                ?? throw new NotFoundException("Student not found");

            await _userService.UpdateAsync(student.UserId, request.User);
        }
    }
}
