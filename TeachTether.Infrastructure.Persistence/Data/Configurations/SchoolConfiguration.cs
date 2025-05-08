using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TeachTether.Domain.Entities;

namespace TeachTether.Infrastructure.Persistence.Data.Configurations
{
    public class SchoolConfiguration : IEntityTypeConfiguration<School>
    {
        public void Configure(EntityTypeBuilder<School> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasOne<SchoolOwner>()
                .WithMany()
                .HasForeignKey(s => s.SchoolOwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(s => new {s.SchoolOwnerId, s.Name })
                .IsUnique();
        }
    }
}
