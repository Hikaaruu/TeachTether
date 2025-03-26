using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TeachTether.Domain.Entities;

namespace TeachTether.Infrastructure.Persistence.Data.Configurations
{
    public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
    {
        public void Configure(EntityTypeBuilder<Teacher> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.UserId)
                .IsRequired();

            builder.HasIndex(t => t.UserId)
                .IsUnique();

            builder.HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<Teacher>(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(t => t.DateOfBirth)
                .IsRequired();

            builder.HasOne<School>()
                .WithMany()
                .HasForeignKey(t => t.SchoolId)
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
