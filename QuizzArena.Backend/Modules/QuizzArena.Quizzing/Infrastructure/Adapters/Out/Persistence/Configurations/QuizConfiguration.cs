using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.Quizzing.Domain.Entities;

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
                .IsRequired().HasColumnType($"{QuizzingConstants.Schema}.quiz_status");

            builder.Property(x => x.Deleted)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnType("timestamptz")
                .IsRequired();
        }
    }
}
