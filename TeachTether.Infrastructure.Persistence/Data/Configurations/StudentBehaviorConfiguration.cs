using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachTether.Domain.Entities;

namespace TeachTether.Infrastructure.Persistence.Data.Configurations;

public class StudentBehaviorConfiguration : IEntityTypeConfiguration<StudentBehavior>
{
    public void Configure(EntityTypeBuilder<StudentBehavior> builder)
    {
        builder.HasKey(sb => sb.Id);

        builder.Property(sb => sb.BehaviorScore)
            .HasColumnType("decimal(4,2)")
            .IsRequired();

        builder.Property(sb => sb.Comment)
            .HasMaxLength(500);

        builder.Property(sb => sb.BehaviorDate).IsRequired();
        builder.Property(sb => sb.CreatedAt).IsRequired();

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(sb => sb.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Teacher>()
            .WithMany()
            .HasForeignKey(sb => sb.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Subject>()
            .WithMany()
            .HasForeignKey(sb => sb.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}