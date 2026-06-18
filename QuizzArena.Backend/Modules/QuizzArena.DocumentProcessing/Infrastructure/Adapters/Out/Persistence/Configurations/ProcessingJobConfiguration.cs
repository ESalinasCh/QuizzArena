using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence.Configurations;

internal sealed class ProcessingJobConfiguration : IEntityTypeConfiguration<ProcessingJob>
{
    public void Configure(EntityTypeBuilder<ProcessingJob> builder)
    {
        builder.ToTable(
            "processing_job",
            schema: DocumentProcessingConstants.Schema
            );

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType($"{DocumentProcessingConstants.Schema}.job_status");

        builder.Property(x => x.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.FinishedAt)
            .HasColumnName("finished_at")
            .HasColumnType("timestamptz");

        builder.HasMany(x => x.DocumentProcessingJobs).WithOne().HasForeignKey(x => x.ProcessingJobId);
    }
}
