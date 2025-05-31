using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services;

public class ClassGroupStudentService(IUnitOfWork unitOfWork, IMapper mapper) : IClassGroupStudentService
{
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task CreateAsync(CreateClassGroupStudentRequest request, int classGroupId)
    {
        if (await _unitOfWork.ClassGroupStudents.AnyAsync(cgs =>
                cgs.StudentId == request.StudentId))
            throw new BadRequestException($"Student {request.StudentId} is already in class group.");

        var classGroupStudent = _mapper.Map<ClassGroupStudent>(request);
        classGroupStudent.ClassGroupId = classGroupId;

        await _unitOfWork.ClassGroupStudents.AddAsync(classGroupStudent);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int classGroupId, int studentId)
    {
        var classGroupStudent = await _unitOfWork.ClassGroupStudents.GetByStudentIdAsync(studentId)
                                ?? throw new NotFoundException("Student is not in this class group");

        if (classGroupStudent.ClassGroupId != classGroupId)
            throw new Exception("Unexpected Behavior occurred");

        _unitOfWork.ClassGroupStudents.Delete(classGroupStudent);
        await _unitOfWork.SaveChangesAsync();
    }
}