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
        public IAnnouncementClassGroupRepository AnnouncementClassGroups { get; }
        public IAnnouncementRepository Announcements { get; }
        public IClassGroupSubjectRepository ClassGroupsSubjects { get; }
        public IMessageAttachmentRepository MessageAttachments { get; }
        public IMessageRepository Messages { get; }
        public IMessageThreadRepository MessageThreads { get; }
        public IStudentAttendanceRepository StudentAttendances { get; }
        public IStudentBehaviorRepository StudentBehaviors { get; }
        public IStudentGradeRepository StudentGrades { get; }
        public ISubjectRepository Subjects { get; }

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
            ITeacherRepository teacherRepository,
            IAnnouncementClassGroupRepository announcementClassGroupRepository,
            IAnnouncementRepository announcementRepository,
            IClassGroupSubjectRepository classGroupSubjectRepository,
            IMessageAttachmentRepository messageAttachmentRepository,
            IMessageRepository messageRepository,
            IMessageThreadRepository messageThreadRepository,
            IStudentAttendanceRepository studentAttendanceRepository,
            IStudentBehaviorRepository studentBehaviorRepository,
            IStudentGradeRepository studentGradeRepository,
            ISubjectRepository subjectRepository)
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
            AnnouncementClassGroups = announcementClassGroupRepository;
            Announcements = announcementRepository;
            ClassGroupsSubjects = classGroupSubjectRepository;
            MessageAttachments = messageAttachmentRepository;
            Messages = messageRepository;
            MessageThreads = messageThreadRepository;
            StudentAttendances = studentAttendanceRepository;
            StudentGrades = studentGradeRepository;
            Subjects = subjectRepository;
            StudentBehaviors = studentBehaviorRepository;
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
