using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachTether.Domain.Entities;

namespace TeachTether.Infrastructure.Persistence.Data.Configurations;

public class ClassGroupConfiguration : IEntityTypeConfiguration<ClassGroup>
{
    public void Configure(EntityTypeBuilder<ClassGroup> builder)
    {
        builder.HasKey(cg => cg.Id);

        builder.Property(cg => cg.GradeYear)
            .IsRequired();

        builder.Property(cg => cg.Section)
            .IsRequired()
            .HasMaxLength(1);

        builder.HasIndex(cg => new { cg.SchoolId, cg.GradeYear, cg.Section })
            .IsUnique();

        builder.HasOne<Teacher>()
            .WithMany()
            .HasForeignKey(cg => cg.HomeroomTeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<School>()
            .WithMany()
            .HasForeignKey(cg => cg.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}