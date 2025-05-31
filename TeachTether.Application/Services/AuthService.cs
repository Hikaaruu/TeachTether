using System.Security.Claims;
using AutoMapper;
using TeachTether.Application.Common;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services;

public class AuthService(
    IUserService userService,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IAuthService
{
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserService _userService = userService;

    public async Task<(OperationResult Result, string? Token)> RegisterAsync(RegisterRequest request)
    {
        var (result, user) = await _userService.RegisterAsync(request);

        if (!result.Succeeded)
            return (result, null);


        var schoolOwner = new SchoolOwner
        {
            UserId = user.Id!
        };
        await _unitOfWork.SchoolOwners.AddAsync(schoolOwner);
        await _unitOfWork.SaveChangesAsync();

        var token = await _userService.TryLoginAsync(new LoginRequest
        {
            UserName = request.UserName,
            Password = request.Password
        });

        return (result, token);
    }

    public async Task<string?> LoginAsync(LoginRequest request)
    {
        return await _userService.TryLoginAsync(request);
    }

    public async Task<UserInfoResponse?> GetCurrentUserInfoAsync(ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return null;

        var dbUser = await _userService.GetByIdAsync(userId);
        if (dbUser == null) return null;

        int? entityId = null;
        int? schoolId = null;

        switch (dbUser.UserType)
        {
            case UserType.Student:
                var student = await _unitOfWork.Students.GetByUserIdAsync(userId);
                entityId = student?.Id;
                schoolId = student?.SchoolId;
                break;
            case UserType.Teacher:
                var teacher = await _unitOfWork.Teachers.GetByUserIdAsync(userId);
                entityId = teacher?.Id;
                schoolId = teacher?.SchoolId;
                break;
            case UserType.Guardian:
                var guardian = await _unitOfWork.Guardians.GetByUserIdAsync(userId);
                entityId = guardian?.Id;
                schoolId = guardian?.SchoolId;
                break;
            case UserType.SchoolAdmin:
                var admin = await _unitOfWork.SchoolAdmins.GetByUserIdAsync(userId);
                entityId = admin?.Id;
                schoolId = admin?.SchoolId;
                break;
            case UserType.SchoolOwner:
                var owner = await _unitOfWork.SchoolOwners.GetByUserIdAsync(userId);
                entityId = owner?.Id;
                schoolId = null; // owners don't have a fixed school
                break;
        }

        if (entityId == null)
            return null;

        var userInfo = _mapper.Map<UserInfoResponse>(dbUser);
        userInfo.EntityId = entityId.Value;
        userInfo.SchoolId = schoolId;

        return userInfo;
    }
}