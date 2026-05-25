using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Configurations;

internal sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable(
       "match",
       QuizzingConstants.Schema);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired().HasColumnType($"{QuizzingConstants.Schema}.match_status");

        builder.Property(x => x.Mode)
            .IsRequired().HasColumnType($"{QuizzingConstants.Schema}.match_mode");

        builder.Property(x => x.TimeMinutes)
            .IsRequired();

        builder.Property(x => x.StartedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.FinishedAt)
            .HasColumnType("timestamptz");

        builder.Property(x => x.CourseId);

        builder.Property(x => x.Deleted)
        .IsRequired()
        .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.DeletedAt)
           .HasColumnType("timestamptz");

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.HasOne<Quiz>()
            .WithMany()
            .HasForeignKey(x => x.QuizId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
