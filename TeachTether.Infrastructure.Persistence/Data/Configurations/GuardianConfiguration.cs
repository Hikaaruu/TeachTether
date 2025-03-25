using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Database;

namespace TeachTether.Infrastructure.Persistence.Data.Configurations
{
    public class GuardianConfiguration : IEntityTypeConfiguration<Guardian>
    {
        public void Configure(EntityTypeBuilder<Guardian> builder)
        {
            builder.HasKey(g => g.Id);

            builder.Property(g => g.UserId)
                .IsRequired();

            builder.HasIndex(g => g.UserId)
                .IsUnique();

            builder.HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<Guardian>(g => g.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(g => g.DateOfBirth)
                .IsRequired();

            builder.HasOne<School>()
                .WithMany()
                .HasForeignKey(g => g.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
