using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Database;

namespace TeachTether.Infrastructure.Persistence.Data.Configurations
{
    public class SchoolOwnerConfiguration : IEntityTypeConfiguration<SchoolOwner>
    {
        public void Configure(EntityTypeBuilder<SchoolOwner> builder)
        {
            builder.HasKey(so => so.Id);

            builder.Property(so => so.UserId)
                .IsRequired();

            builder.HasIndex(so => so.UserId)
                .IsUnique();

            builder.HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<SchoolOwner>(so => so.UserId)
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
