using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories;

public class UnitOfWork(
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
    ISubjectRepository subjectRepository) : IUnitOfWork
{
    private readonly ApplicationDbContext _context = context;

    public IClassAssignmentRepository ClassAssignments { get; } = classAssignmentRepository;
    public IClassGroupRepository ClassGroups { get; } = classGroupRepository;
    public IClassGroupStudentRepository ClassGroupStudents { get; } = classGroupStudentRepository;
    public IGuardianRepository Guardians { get; } = guardianRepository;
    public IGuardianStudentRepository GuardianStudents { get; } = guardianStudentRepository;
    public ISchoolAdminRepository SchoolAdmins { get; } = schoolAdminRepository;
    public ISchoolOwnerRepository SchoolOwners { get; } = schoolOwnerRepository;
    public ISchoolRepository Schools { get; } = schoolRepository;
    public IStudentRepository Students { get; } = studentRepository;
    public ITeacherRepository Teachers { get; } = teacherRepository;
    public IAnnouncementClassGroupRepository AnnouncementClassGroups { get; } = announcementClassGroupRepository;
    public IAnnouncementRepository Announcements { get; } = announcementRepository;
    public IClassGroupSubjectRepository ClassGroupsSubjects { get; } = classGroupSubjectRepository;
    public IMessageAttachmentRepository MessageAttachments { get; } = messageAttachmentRepository;
    public IMessageRepository Messages { get; } = messageRepository;
    public IMessageThreadRepository MessageThreads { get; } = messageThreadRepository;
    public IStudentAttendanceRepository StudentAttendances { get; } = studentAttendanceRepository;
    public IStudentBehaviorRepository StudentBehaviors { get; } = studentBehaviorRepository;
    public IStudentGradeRepository StudentGrades { get; } = studentGradeRepository;
    public ISubjectRepository Subjects { get; } = subjectRepository;

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}