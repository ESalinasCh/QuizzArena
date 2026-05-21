using Microsoft.EntityFrameworkCore;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;
using QuizzArena.DocumentProcessing.Infraestructure.Adapters.Out.Persistence;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence
{
    internal class DocumentProcessingDbContext : DbContext
    {
        public DocumentProcessingDbContext(
            DbContextOptions<DocumentProcessingDbContext> options)
            : base(options)
        {
        }

        public DbSet<ClassSource> ClassSource => Set<ClassSource>();
        public DbSet<DocumentChunk> DocumentChunk => Set<DocumentChunk>();
        public DbSet<DocumentProcessingJob> DocumentProcessingJob => Set<DocumentProcessingJob>();
        public DbSet<ProcessingJob> CourseStudents => Set<ProcessingJob>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // user_management schema
            modelBuilder.HasDefaultSchema(DocumentProcessingConstants.Schema);

            modelBuilder.HasPostgresEnum<JobStatus>(schema: DocumentProcessingConstants.Schema, name: "job_status");
            modelBuilder.HasPostgresEnum<SourceStatus>(schema: DocumentProcessingConstants.Schema, name: "source_status");
            modelBuilder.HasPostgresEnum<SourceType>(schema: DocumentProcessingConstants.Schema, name: "source_type");

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(DocumentProcessingDbContext).Assembly
            );
        }
    }
}
