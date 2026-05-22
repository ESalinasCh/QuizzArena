using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Infraestructure.Adapters.Out.Persistence;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence.Configurations
{
    internal class DocumentProcessingJobConfiguration : IEntityTypeConfiguration<DocumentProcessingJob>
    {
        public void Configure(EntityTypeBuilder<DocumentProcessingJob> builder)
        {
            builder.ToTable(
                "course_students",
                schema: DocumentProcessingConstants.Schema
                );

            builder.HasKey(x => x.Id);

            builder.HasOne<ClassSource>()
                .WithMany()
                .HasForeignKey(x => x.DocumentId)
                .HasConstraintName("FK_ProcessingJob_ClassSource_DocumentId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ProcessingJob>()
                .WithMany()
                .HasForeignKey(x => x.ProcessingJobId)
                .HasConstraintName("FK_ClassSource_ProcessingJob_ProcessingJobId")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
