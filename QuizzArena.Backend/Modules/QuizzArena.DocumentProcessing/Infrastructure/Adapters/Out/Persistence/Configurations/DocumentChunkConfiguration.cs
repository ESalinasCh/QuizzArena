using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Infraestructure.Adapters.Out.Persistence;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence.Configurations
{
    internal class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
    {
        public void Configure(EntityTypeBuilder<DocumentChunk> builder)
        {
            builder.ToTable(
                "document_chunk",
                schema: DocumentProcessingConstants.Schema
                );

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ChunkOrder)
                .HasColumnName("chunk_order")
                .HasConversion<int>();

            builder.Property(x => x.Content)
                .HasColumnName("content")
                .HasConversion<string>();

            builder.Property(x => x.Embedding)
                .HasColumnName("embedding")
                .HasColumnType("vector(768)")
                .IsRequired();


            // FK
            builder.HasOne<ClassSource>()
                .WithMany()
                .HasForeignKey(x => x.DocumentId)
                .HasConstraintName("FK_DocumentChunk_ClassSource_DocumentId")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
