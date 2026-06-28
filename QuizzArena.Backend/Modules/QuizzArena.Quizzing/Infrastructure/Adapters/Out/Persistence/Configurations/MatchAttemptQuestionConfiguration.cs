using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Configurations;

internal sealed class MatchAttemptQuestionConfiguration : IEntityTypeConfiguration<MatchAttemptQuestion>
{
    public void Configure(EntityTypeBuilder<MatchAttemptQuestion> builder)
    {
        builder.ToTable(
      "match_attempt_question",
      QuizzingConstants.Schema);

        builder.Property(x => x.ValueScore)
            .HasPrecision(5, 2);
    }
}
