using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence.Configurations;

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
            .HasConversion<int>();

        builder.Property(x => x.Content)
            .HasColumnName("content")
            .HasColumnType("text");

        builder.Property(x => x.Embedding)
            .HasColumnType("vector(1024)")
            .IsRequired();

    }
}
