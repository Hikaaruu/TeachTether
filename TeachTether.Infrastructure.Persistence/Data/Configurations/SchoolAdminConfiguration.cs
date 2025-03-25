using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Database;

namespace TeachTether.Infrastructure.Persistence.Data.Configurations
{
    public class SchoolAdminConfiguration : IEntityTypeConfiguration<SchoolAdmin>
    {
        public void Configure(EntityTypeBuilder<SchoolAdmin> builder)
        {
            builder.HasKey(sa => sa.Id);

            builder.Property(sa => sa.UserId)
                .IsRequired();

            builder.HasIndex(sa => sa.UserId)
                .IsUnique();

            builder.HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<SchoolAdmin>(sa => sa.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<School>()
                .WithMany()
                .HasForeignKey(sa => sa.SchoolId)
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
