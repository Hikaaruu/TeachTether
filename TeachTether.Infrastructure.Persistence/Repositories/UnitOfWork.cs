using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IClassAssignmentRepository ClassAssignments { get; }
        public IClassGroupRepository ClassGroups { get; }
        public IClassGroupStudentRepository ClassGroupStudents { get; }
        public IGuardianRepository Guardians { get; }
        public IGuardianStudentRepository GuardianStudents { get; }
        public ISchoolAdminRepository SchoolAdmins { get; }
        public ISchoolOwnerRepository SchoolOwners { get; }
        public ISchoolRepository Schools { get; }
        public IStudentRepository Students { get; }
        public ITeacherRepository Teachers { get; }

        public UnitOfWork(
            ApplicationDbContext context,
            IClassAssignmentRepository classAssignmentRepository,
            IClassGroupRepository classGroupRepository,
            IClassGroupStudentRepository classGroupStudentRepository,
            IGuardianRepository guardianRepository,
            IGuardianStudentRepository guardianStudentRepository,
            ISchoolAdminRepository schoolAdminRepository,
            ISchoolOwnerRepository schoolOwnerRepository,
            ISchoolRepository schoolRepository,
            IStudentRepository studentRepository,
            ITeacherRepository teacherRepository)
        {
            _context = context;
            ClassAssignments = classAssignmentRepository;
            ClassGroups = classGroupRepository;
            ClassGroupStudents = classGroupStudentRepository;
            Guardians = guardianRepository;
            GuardianStudents = guardianStudentRepository;
            SchoolAdmins = schoolAdminRepository;
            SchoolOwners = schoolOwnerRepository;
            Schools = schoolRepository;
            Students = studentRepository;
            Teachers = teacherRepository;
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
