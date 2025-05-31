using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services;

public class ClassAssignmentService(IUnitOfWork unitOfWork, IMapper mapper) : IClassAssignmentService
{
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task CreateAsync(CreateClassAssignmentRequest request, int classGroupId, int subjectId)
    {
        var classGroupSubject = (await _unitOfWork.ClassGroupsSubjects
                                    .GetByClassGroupIdAsync(classGroupId))
                                .SingleOrDefault(cgs => cgs.SubjectId == subjectId)
                                ?? throw new NotFoundException("This subject is not assigned to specified class group");

        if (await _unitOfWork.ClassAssignments.AnyAsync(ca =>
                ca.ClassGroupSubjectId == classGroupSubject.Id &&
                ca.TeacherId == request.TeacherId))
            throw new BadRequestException(
                $"Teacher {request.TeacherId} is already assigned to class group {classGroupId} for teaching subject {subjectId}.");

        var classAssignment = _mapper.Map<ClassAssignment>(request);
        classAssignment.ClassGroupSubjectId = classGroupSubject.Id;

        await _unitOfWork.ClassAssignments.AddAsync(classAssignment);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int classGroupId, int subjectId, int teacherId)
    {
        var classGroupSubject = (await _unitOfWork.ClassGroupsSubjects
                                    .GetByClassGroupIdAsync(classGroupId))
                                .SingleOrDefault(cgs => cgs.SubjectId == subjectId)
                                ?? throw new NotFoundException("This subject is not assigned to specified class group");

        var classAssignment = (await _unitOfWork.ClassAssignments
                                  .GetByClassGroupSubjectIdAsync(classGroupSubject.Id))
                              .SingleOrDefault(ca => ca.TeacherId == teacherId)
                              ?? throw new NotFoundException("Class assignment not found");


        _unitOfWork.ClassAssignments.Delete(classAssignment);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<ClassAssignmentResponse>> GetByTeacherAsync(int teacherId)
    {
        var assignments = await _unitOfWork.ClassAssignments.GetByTeacherIdAsync(teacherId);
        var result = new List<ClassAssignmentResponse>();
        foreach (var assignment in assignments)
        {
            var cgs = await _unitOfWork.ClassGroupsSubjects.GetByIdAsync(assignment.ClassGroupSubjectId) ??
                      throw new Exception();
            var classGroup = await _unitOfWork.ClassGroups.GetByIdAsync(cgs.ClassGroupId) ?? throw new Exception();
            var subject = await _unitOfWork.Subjects.GetByIdAsync(cgs.SubjectId) ?? throw new Exception();
            var assignmentResponse = new ClassAssignmentResponse
            {
                ClassGroupId = classGroup.Id,
                ClassGroupName = $"{classGroup.GradeYear} - {classGroup.Section}",
                SubjectId = cgs.SubjectId,
                SubjectName = subject.Name
            };

            result.Add(assignmentResponse);
        }

        return result;
    }
}