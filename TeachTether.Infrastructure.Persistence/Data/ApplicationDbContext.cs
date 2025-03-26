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

        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Guardian> Guardians { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<SchoolAdmin> SchoolAdmins { get; set; }
        public DbSet<SchoolOwner> SchoolOwners { get; set; }
        public DbSet<ClassGroup> ClassGroups { get; set; }
        public DbSet<ClassAssignment> ClassAssignments { get; set; }
        public DbSet<ClassGroupStudent> ClassGroupStudents { get; set; }
        public DbSet<GuardianStudent> GuardianStudents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ClassAssignmentConfiguration());
            modelBuilder.ApplyConfiguration(new ClassGroupConfiguration());
            modelBuilder.ApplyConfiguration(new ClassGroupStudentConfiguration());
            modelBuilder.ApplyConfiguration(new GuardianConfiguration());
            modelBuilder.ApplyConfiguration(new GuardianStudentConfiguration());
            modelBuilder.ApplyConfiguration(new SchoolConfiguration());
            modelBuilder.ApplyConfiguration(new SchoolAdminConfiguration());
            modelBuilder.ApplyConfiguration(new SchoolOwnerConfiguration());
            modelBuilder.ApplyConfiguration(new StudentConfiguration());
            modelBuilder.ApplyConfiguration(new TeacherConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
        }
    }
}
