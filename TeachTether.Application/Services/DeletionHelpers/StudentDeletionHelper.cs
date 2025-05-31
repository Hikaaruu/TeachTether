using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;

namespace TeachTether.Application.Services.DeletionHelpers;

public class StudentDeletionHelper(IUnitOfWork unitOfWork, IUserRepository userRepository) : IStudentDeletionHelper
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task DeleteStudentAsync(int id)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(id)
                      ?? throw new NotFoundException("Student not found");

        var grades = await _unitOfWork.StudentGrades.GetByStudentIdAsync(id);
        _unitOfWork.StudentGrades.DeleteMany(grades);
        var att = await _unitOfWork.StudentAttendances.GetByStudentIdAsync(id);
        _unitOfWork.StudentAttendances.DeleteMany(att);
        var beh = await _unitOfWork.StudentBehaviors.GetByStudentIdAsync(id);
        _unitOfWork.StudentBehaviors.DeleteMany(beh);

        var guardStudents = await _unitOfWork.GuardianStudents.GetByStudentIdAsync(id);
        _unitOfWork.GuardianStudents.DeleteMany(guardStudents);

        var cgStudent = await _unitOfWork.ClassGroupStudents.GetByStudentIdAsync(id);
        if (cgStudent is not null) _unitOfWork.ClassGroupStudents.Delete(cgStudent);

        _unitOfWork.Students.Delete(student);

        var result = await _userRepository.DeleteAsync(student.UserId);

        if (result.Succeeded)
            await _unitOfWork.SaveChangesAsync();
        else
            throw new Exception();
    }
}