using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Configurations
{
    internal sealed class QuizConfiguration : IEntityTypeConfiguration<Quiz>
    {
        public void Configure(EntityTypeBuilder<Quiz> builder)
        {
            builder.ToTable(
            "quiz",
            QuizzingConstants.Schema);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired().HasColumnType($"{QuizzingConstants.Schema}.quiz_status")
                .HasDefaultValueSql($"'{QuizStatus.published.ToString().ToLower()}'::{QuizzingConstants.Schema}.quiz_status");

            builder.Property(x => x.Deleted)
                .IsRequired()
                .HasDefaultValue(false); ;

            builder.Property(x => x.CreatedAt)
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(x => x.DeletedAt)
            .HasColumnType("timestamptz");

            builder.HasMany(x => x.QuizQuestions).WithOne().HasForeignKey(x => x.QuizId);
        }
    }
}
