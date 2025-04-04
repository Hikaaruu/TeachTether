using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data.Configurations;

namespace TeachTether.Infrastructure.Persistence.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<AnnouncementClassGroup> AnnouncementClassGroups { get; set; }
        public DbSet<ClassAssignment> ClassAssignments { get; set; }
        public DbSet<ClassGroup> ClassGroups { get; set; }
        public DbSet<ClassGroupStudent> ClassGroupStudents { get; set; }
        public DbSet<ClassGroupSubject> ClassGroupSubjects { get; set; }
        public DbSet<Guardian> Guardians { get; set; }
        public DbSet<GuardianStudent> GuardianStudents { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageAttachment> MessageAttachments { get; set; }
        public DbSet<MessageThread> MessageThreads { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<SchoolAdmin> SchoolAdmins { get; set; }
        public DbSet<SchoolOwner> SchoolOwners { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentAttendance> StudentAttendances { get; set; }
        public DbSet<StudentBehavior> StudentBehaviors { get; set; }
        public DbSet<StudentGrade> StudentGrades { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new AnnouncementClassGroupConfiguration());
            modelBuilder.ApplyConfiguration(new AnnouncementConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
            modelBuilder.ApplyConfiguration(new ClassAssignmentConfiguration());
            modelBuilder.ApplyConfiguration(new ClassGroupConfiguration());
            modelBuilder.ApplyConfiguration(new ClassGroupStudentConfiguration());
            modelBuilder.ApplyConfiguration(new ClassGroupSubjectConfiguration());
            modelBuilder.ApplyConfiguration(new GuardianConfiguration());
            modelBuilder.ApplyConfiguration(new GuardianStudentConfiguration());
            modelBuilder.ApplyConfiguration(new MessageAttachmentConfiguration());
            modelBuilder.ApplyConfiguration(new MessageConfiguration());
            modelBuilder.ApplyConfiguration(new MessageThreadConfiguration());
            modelBuilder.ApplyConfiguration(new SchoolAdminConfiguration());
            modelBuilder.ApplyConfiguration(new SchoolConfiguration());
            modelBuilder.ApplyConfiguration(new SchoolOwnerConfiguration());
            modelBuilder.ApplyConfiguration(new StudentAttendanceConfiguration());
            modelBuilder.ApplyConfiguration(new StudentBehaviorConfiguration());
            modelBuilder.ApplyConfiguration(new StudentConfiguration());
            modelBuilder.ApplyConfiguration(new StudentGradeConfiguration());
            modelBuilder.ApplyConfiguration(new SubjectConfiguration());
            modelBuilder.ApplyConfiguration(new TeacherConfiguration());
        }
    }
}
