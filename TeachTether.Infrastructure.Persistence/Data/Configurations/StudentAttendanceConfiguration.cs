using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachTether.Domain.Entities;

namespace TeachTether.Infrastructure.Persistence.Data.Configurations;

public class StudentAttendanceConfiguration : IEntityTypeConfiguration<StudentAttendance>
{
    public void Configure(EntityTypeBuilder<StudentAttendance> builder)
    {
        builder.HasKey(sa => sa.Id);

        builder.Property(sa => sa.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(sa => sa.AttendanceDate).IsRequired();
        builder.Property(sa => sa.CreatedAt).IsRequired();

        builder.Property(sa => sa.Comment)
            .HasMaxLength(500);

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(sa => sa.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Teacher>()
            .WithMany()
            .HasForeignKey(sa => sa.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Subject>()
            .WithMany()
            .HasForeignKey(sa => sa.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}