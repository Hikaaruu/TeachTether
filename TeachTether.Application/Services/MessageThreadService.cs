using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services;

public class MessageThreadService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IUserService userService,
    IMessageThreadDeletionHelper messageThreadDeletionHelper) : IMessageThreadService
{
    private readonly IMapper _mapper = mapper;
    private readonly IMessageThreadDeletionHelper _messageThreadDeletionHelper = messageThreadDeletionHelper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserService _userService = userService;

    public async Task<MessageThreadResponse> CreateAsync(CreateMessageThreadRequest request)
    {
        if (await _unitOfWork.MessageThreads.AnyAsync(mt =>
                mt.TeacherId == request.TeacherId &&
                mt.GuardianId == request.GuardianId))
            throw new BadRequestException(
                $"Message thread with Teacher {request.TeacherId} and Guardian {request.GuardianId} already exists.");

        var messageThread = _mapper.Map<MessageThread>(request);
        messageThread.CreatedAt = DateTime.Now;

        await _unitOfWork.MessageThreads.AddAsync(messageThread);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<MessageThreadResponse>(messageThread);
    }

    public async Task DeleteAsync(int id)
    {
        await _messageThreadDeletionHelper.DeleteMessageThreadAsync(id);
    }

    public async Task<IEnumerable<MessageThreadResponse>> GetAllForUserAsync(string userId)
    {
        var user = await _userService.GetByIdAsync(userId)
                   ?? throw new NotFoundException("User not found");

        IEnumerable<MessageThread> threads;

        switch (user.UserType)
        {
            case UserType.Teacher:
            {
                var teacher = await _unitOfWork.Teachers.GetByUserIdAsync(userId)
                              ?? throw new NotFoundException("Teacher not found");

                threads = await _unitOfWork.MessageThreads.GetByTeacherIdAsync(teacher.Id);
                break;
            }

            case UserType.Guardian:
            {
                var guardian = await _unitOfWork.Guardians.GetByUserIdAsync(userId)
                               ?? throw new NotFoundException("Guardian not found");

                threads = await _unitOfWork.MessageThreads.GetByGuardianIdAsync(guardian.Id);
                break;
            }

            default:
                throw new Exception("Unexpected behavior occurred");
        }


        return _mapper.Map<IEnumerable<MessageThreadResponse>>(threads);
    }

    public async Task<MessageThreadResponse> GetByIdAsync(int id)
    {
        var thread = await _unitOfWork.MessageThreads.GetByIdAsync(id)
                     ?? throw new NotFoundException("thread not found");

        return _mapper.Map<MessageThreadResponse>(thread);
    }
}