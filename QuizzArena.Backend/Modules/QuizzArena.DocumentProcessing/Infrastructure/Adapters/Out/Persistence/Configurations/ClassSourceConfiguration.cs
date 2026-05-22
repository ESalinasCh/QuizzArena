using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;
using QuizzArena.DocumentProcessing.Infraestructure.Adapters.Out.Persistence;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence.Configurations
{
    internal class ClassSourceConfiguration : IEntityTypeConfiguration<ClassSource>
    {
        public void Configure(EntityTypeBuilder<ClassSource> builder)
        {
            builder.ToTable(
                "class_source",
                schema: DocumentProcessingConstants.Schema
                );

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .HasColumnName("type")
                .HasColumnType($"{DocumentProcessingConstants.Schema}.source_type");

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasColumnType($"{DocumentProcessingConstants.Schema}.job_status");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.TranscriptUrl)
                .HasColumnName("transcript_url")
                .HasMaxLength(255);

            builder.Property(x => x.Deleted)
                .HasColumnName("deleted")
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamptz");

            builder.Property(x => x.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("timestamptz");

            // FK
            builder.Property(x => x.CourseId)
                .HasColumnName("course_id")
                .IsRequired();

            builder.Property(x => x.UserId)
                .HasColumnName("user_d")
                .IsRequired();
        }
    }
}
