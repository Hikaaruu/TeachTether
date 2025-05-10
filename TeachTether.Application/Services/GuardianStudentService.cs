using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services
{
    public class GuardianStudentService : IGuardianStudentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GuardianStudentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateAsync(int studentId, int guardianId)
        {
            if (await _unitOfWork.GuardianStudents.AnyAsync(gs =>
            gs.StudentId == studentId &&
            gs.GuardianId == guardianId))
            {
                throw new BadRequestException($"Student {studentId} is already assigned to guardian {guardianId}.");
            }

            var guardianStudent = new GuardianStudent()
            {
                StudentId = studentId,
                GuardianId = guardianId
            };

            await _unitOfWork.GuardianStudents.AddAsync(guardianStudent);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int studentId, int guardianId)
        {
            var guardianStudent = (await _unitOfWork.GuardianStudents
                .GetByStudentIdAsync(studentId))
                .SingleOrDefault(gs => gs.GuardianId == guardianId)
                ?? throw new NotFoundException("Student is not assigned to this guardian");

            _unitOfWork.GuardianStudents.Delete(guardianStudent);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
