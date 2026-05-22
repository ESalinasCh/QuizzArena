using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence;


namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Configurations
{
    internal sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.ToTable(
            "question",
            QuizzingConstants.Schema);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired().HasColumnType($"{QuizzingConstants.Schema}.question_status");

            builder.Property(x => x.Type)
                .IsRequired().HasColumnType($"{QuizzingConstants.Schema}.question_type");

            builder.Property(x => x.WasModified)
                .IsRequired();

            builder.Property(x => x.Deleted)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(x => x.ProcessingJobId);

            builder.HasIndex(x => x.Status);

            builder.HasIndex(x => x.Type);

            builder.HasIndex(x => x.Deleted);

            builder.HasMany(x => x.Options)
            .WithOne()
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    
    }
}
