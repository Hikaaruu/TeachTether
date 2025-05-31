using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachTether.Domain.Entities;

namespace TeachTether.Infrastructure.Persistence.Data.Configurations;

public class AnnouncementClassGroupConfiguration : IEntityTypeConfiguration<AnnouncementClassGroup>
{
    public void Configure(EntityTypeBuilder<AnnouncementClassGroup> builder)
    {
        builder.HasKey(ac => ac.Id);

        builder.HasOne<Announcement>()
            .WithMany()
            .HasForeignKey(a => a.AnnouncementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ClassGroup>()
            .WithMany()
            .HasForeignKey(a => a.ClassGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.AnnouncementId, a.ClassGroupId })
            .IsUnique();
    }
}