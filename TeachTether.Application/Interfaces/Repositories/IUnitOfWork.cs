namespace TeachTether.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IAnnouncementClassGroupRepository AnnouncementClassGroups { get; }
    IAnnouncementRepository Announcements { get; }
    IClassAssignmentRepository ClassAssignments { get; }
    IClassGroupRepository ClassGroups { get; }
    IClassGroupStudentRepository ClassGroupStudents { get; }
    IClassGroupSubjectRepository ClassGroupsSubjects { get; }
    IGuardianRepository Guardians { get; }
    IGuardianStudentRepository GuardianStudents { get; }
    IMessageAttachmentRepository MessageAttachments { get; }
    IMessageRepository Messages { get; }
    IMessageThreadRepository MessageThreads { get; }
    ISchoolAdminRepository SchoolAdmins { get; }
    ISchoolOwnerRepository SchoolOwners { get; }
    ISchoolRepository Schools { get; }
    IStudentAttendanceRepository StudentAttendances { get; }
    IStudentBehaviorRepository StudentBehaviors { get; }
    IStudentGradeRepository StudentGrades { get; }
    IStudentRepository Students { get; }
    ISubjectRepository Subjects { get; }
    ITeacherRepository Teachers { get; }

    Task<int> SaveChangesAsync();
}