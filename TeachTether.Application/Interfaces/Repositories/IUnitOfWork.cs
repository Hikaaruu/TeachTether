namespace TeachTether.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IClassAssignmentRepository ClassAssignments { get; }
        IClassGroupRepository ClassGroups { get; }
        IClassGroupStudentRepository ClassGroupStudents { get; }
        IGuardianRepository Guardians { get; }
        IGuardianStudentRepository GuardianStudents { get; }
        ISchoolAdminRepository SchoolAdmins { get; }
        ISchoolOwnerRepository SchoolOwners { get; }
        ISchoolRepository Schools { get; }
        IStudentRepository Students { get; }
        ITeacherRepository Teachers { get; }

        Task<int> SaveChangesAsync();
    }
}
