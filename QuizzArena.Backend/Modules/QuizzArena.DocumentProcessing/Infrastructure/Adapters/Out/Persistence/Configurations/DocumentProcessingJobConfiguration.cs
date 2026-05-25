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
                "document_processing_job",
                schema: DocumentProcessingConstants.Schema
                );

            builder.HasKey(x => x.Id);

            builder.HasOne<ClassSource>()
                .WithMany()
                .HasForeignKey(x => x.DocumentId)
                .HasConstraintName("FK_ProcessingJob_ClassSource_DocumentId")
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
