using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TeachTether.Domain.Entities;

namespace TeachTether.Infrastructure.Persistence.Data.Configurations
{
    public class MessageAttachmentConfiguration : IEntityTypeConfiguration<MessageAttachment>
    {
        public void Configure(EntityTypeBuilder<MessageAttachment> builder)
        {
            builder.HasKey(ma => ma.Id);

            builder.Property(ma => ma.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(ma => ma.FileType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ma => ma.FileUrl)
                .IsRequired()
                .HasMaxLength(512);

            builder.Property(ma => ma.FileSizeBytes)
                .IsRequired();

            builder.Property(ma => ma.UploadedAt)
                .IsRequired();

            builder.HasOne<Message>()
                .WithMany()
                .HasForeignKey(ma => ma.MessageId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(ma => ma.FileUrl).IsUnique();
        }
    }

}
