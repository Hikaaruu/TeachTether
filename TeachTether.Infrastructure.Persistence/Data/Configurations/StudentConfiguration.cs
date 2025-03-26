using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TeachTether.Domain.Entities;

namespace TeachTether.Infrastructure.Persistence.Data.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.UserId)
                .IsRequired();

            builder.HasIndex(s => s.UserId)
                .IsUnique();

            builder.HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(s => s.DateOfBirth)
                .IsRequired();

            builder.HasOne<School>()
                .WithMany()
                .HasForeignKey(s => s.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
