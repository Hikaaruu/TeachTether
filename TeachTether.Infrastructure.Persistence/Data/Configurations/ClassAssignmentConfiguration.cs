using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TeachTether.Domain.Entities;

namespace TeachTether.Infrastructure.Persistence.Data.Configurations
{
    public class ClassAssignmentConfiguration : IEntityTypeConfiguration<ClassAssignment>
    {
        public void Configure(EntityTypeBuilder<ClassAssignment> builder)
        {
            builder.HasKey(ca => ca.Id);

            builder.HasOne<ClassGroupSubject>()
                .WithMany()
                .HasForeignKey(ca => ca.ClassGroupSubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Teacher>()
                .WithMany()
                .HasForeignKey(ca => ca.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(ca => new { ca.ClassGroupSubjectId, ca.TeacherId })
                .IsUnique();
        }
    }
}
