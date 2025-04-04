using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TeachTether.Domain.Entities;

namespace TeachTether.Infrastructure.Persistence.Data.Configurations
{
    public class ClassGroupSubjectConfiguration : IEntityTypeConfiguration<ClassGroupSubject>
    {
        public void Configure(EntityTypeBuilder<ClassGroupSubject> builder)
        {
            builder.HasKey(cgs => cgs.Id);

            builder.HasOne<ClassGroup>()
                .WithMany()
                .HasForeignKey(cgs => cgs.ClassGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Subject>()
              .WithMany()
              .HasForeignKey(cgs => cgs.SubjectId)
              .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(cgs => new { cgs.ClassGroupId, cgs.SubjectId })
                .IsUnique();
        }
    }

}
