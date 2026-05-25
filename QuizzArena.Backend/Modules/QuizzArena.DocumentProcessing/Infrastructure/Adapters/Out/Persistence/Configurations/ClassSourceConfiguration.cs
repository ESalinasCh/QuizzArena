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
                .HasColumnType($"{DocumentProcessingConstants.Schema}.source_type");

            builder.Property(x => x.Status)
                .HasColumnType($"{DocumentProcessingConstants.Schema}.source_status")
                .HasDefaultValueSql($"'{SourceStatus.Pending.ToString().ToLower()}'::{DocumentProcessingConstants.Schema}.source_status");

            builder.Property(x => x.Name)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.TranscriptUrl)
                .HasMaxLength(255);

            builder.Property(x => x.Deleted)
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

            builder.Property(x => x.CourseId)
                .HasColumnName("course_id")
                .IsRequired();

            builder.Property(x => x.UserId)
                .HasColumnName("user_d")
                .IsRequired();

            builder.HasMany(x => x.DocumentChunks).WithOne().HasForeignKey(x => x.DocumentId);
        }
    }
}