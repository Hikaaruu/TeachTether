using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachTether.Domain.Entities;

namespace TeachTether.Infrastructure.Persistence.Data.Configurations;

public class StudentGradeConfiguration : IEntityTypeConfiguration<StudentGrade>
{
    public void Configure(EntityTypeBuilder<StudentGrade> builder)
    {
        builder.HasKey(sg => sg.Id);

        builder.Property(sg => sg.GradeValue)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(g => g.GradeType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(sg => sg.Comment)
            .HasMaxLength(500);

        builder.Property(sg => sg.GradeDate).IsRequired();
        builder.Property(sg => sg.CreatedAt).IsRequired();


        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(sg => sg.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Teacher>()
            .WithMany()
            .HasForeignKey(sg => sg.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Subject>()
            .WithMany()
            .HasForeignKey(sg => sg.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}