using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachTether.Domain.Entities;

namespace TeachTether.Infrastructure.Persistence.Data.Configurations;

public class MessageThreadConfiguration : IEntityTypeConfiguration<MessageThread>
{
    public void Configure(EntityTypeBuilder<MessageThread> builder)
    {
        builder.HasKey(mt => mt.Id);

        builder.Property(mt => mt.CreatedAt).IsRequired();

        builder.HasOne<Teacher>()
            .WithMany()
            .HasForeignKey(mt => mt.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Guardian>()
            .WithMany()
            .HasForeignKey(mt => mt.GuardianId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(mt => new { mt.TeacherId, mt.GuardianId })
            .IsUnique();
    }
}