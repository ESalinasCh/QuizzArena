using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Configurations
{
    internal sealed class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
    {
        public void Configure(EntityTypeBuilder<QuizAttempt> builder)
        {
            builder.ToTable("quiz_attempt", QuizzingConstants.Schema);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Nickname)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.StartDateTime)
                .HasColumnType("timestamptz");

            builder.Property(x => x.EndDateTime)
                .HasColumnType("timestamptz");

            builder.Property(x => x.JoinedAt)
                .HasColumnType("timestamptz");

            builder.Property(x => x.Status).HasColumnType($"{QuizzingConstants.Schema}.quiz_attempt_status")
                .HasDefaultValueSql($"'in_progress'::{QuizzingConstants.Schema}.quiz_attempt_status");

            builder.Property(x => x.Score);
            builder.Property(x => x.UserId);

            builder.HasOne<Match>()
                .WithMany()
                .HasForeignKey(x => x.MatchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Answers).WithOne().HasForeignKey(x => x.QuizAttemptId);
        }
    }
}