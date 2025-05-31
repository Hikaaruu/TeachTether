using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachTether.Domain.Entities;

namespace TeachTether.Infrastructure.Persistence.Data.Configurations;

public class ClassGroupStudentConfiguration : IEntityTypeConfiguration<ClassGroupStudent>
{
    public void Configure(EntityTypeBuilder<ClassGroupStudent> builder)
    {
        builder.HasKey(cgs => cgs.Id);

        builder.HasOne<Student>()
            .WithOne()
            .HasForeignKey<ClassGroupStudent>(cgs => cgs.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ClassGroup>()
            .WithMany()
            .HasForeignKey(cgs => cgs.ClassGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cgs => cgs.StudentId)
            .IsUnique();
    }
}