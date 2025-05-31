using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachTether.Domain.Entities;

namespace TeachTether.Infrastructure.Persistence.Data.Configurations;

public class GuardianStudentConfiguration : IEntityTypeConfiguration<GuardianStudent>
{
    public void Configure(EntityTypeBuilder<GuardianStudent> builder)
    {
        builder.HasKey(gs => gs.Id);

        builder.HasIndex(gs => new { gs.GuardianId, gs.StudentId })
            .IsUnique();

        builder.HasOne<Guardian>()
            .WithMany()
            .HasForeignKey(gs => gs.GuardianId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(gs => gs.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}