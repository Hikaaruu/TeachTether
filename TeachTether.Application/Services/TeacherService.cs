using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public TeacherService(IUnitOfWork unitOfWork, IUserService userService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<CreatedTeacherResponse> CreateAsync(CreateTeacherRequest request, int schoolId)
        {
            (var user, var password) = await _userService.CreateAsync(request.User, UserType.Teacher);

            var teacher = new Teacher
            {
                UserId = user.Id!,
                DateOfBirth = request.DateOfBirth,
                SchoolId = schoolId
            };

            await _unitOfWork.Teachers.AddAsync(teacher);
            await _unitOfWork.SaveChangesAsync();

            return new CreatedTeacherResponse
            {
                User = _mapper.Map<UserDto>(user),
                Id = teacher.Id,
                Username = user.UserName,
                Password = password,
                SchoolId = teacher.SchoolId,
                DateOfBirth = teacher.DateOfBirth
            };
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TeacherResponse>> GetAllBySchoolAsync(int schoolId)
        {
            var teachers = await _unitOfWork.Teachers.GetBySchoolIdAsync(schoolId);

            var userIds = teachers.Select(s => s.UserId);
            var users = await _userService.GetByIdsAsync(userIds);
            var userMap = users
                .Where(u => u.Id != null)
                .ToDictionary(u => u.Id!);

            var result = new List<TeacherResponse>();

            foreach (var teacher in teachers)
            {
                if (!userMap.TryGetValue(teacher.UserId, out var user))
                    throw new Exception($"User data can not be found for teacher with id = {teacher.Id}");

                var response = new TeacherResponse
                {
                    User = _mapper.Map<UserDto>(user),
                    Id = teacher.Id,
                    SchoolId = teacher.SchoolId,
                    DateOfBirth = teacher.DateOfBirth
                };

                result.Add(response);
            }

            return result;
        }

        public async Task<TeacherResponse> GetByIdAsync(int id)
        {
            var teacher = await _unitOfWork.Teachers.GetByIdAsync(id)
                ?? throw new NotFoundException("Teacher not found");

            var user = await _userService.GetByIdAsync(teacher.UserId);

            return new TeacherResponse()
            {
                User = _mapper.Map<UserDto>(user),
                Id = teacher.Id,
                SchoolId = teacher.SchoolId,
                DateOfBirth = teacher.DateOfBirth
            };
        }

        public async Task UpdateAsync(int id, UpdateTeacherRequest request)
        {
            var teacher = await _unitOfWork.Teachers.GetByIdAsync(id)
               ?? throw new NotFoundException("Teacher not found");

            await _userService.UpdateAsync(teacher.UserId, request.User);

            teacher.DateOfBirth = request.DateOfBirth;
            _unitOfWork.Teachers.Update(teacher);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
