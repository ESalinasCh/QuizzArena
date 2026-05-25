using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence;


namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Configurations
{
    internal sealed class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
    {
        public void Configure(EntityTypeBuilder<QuizQuestion> builder)
        {
            builder.ToTable(
            "quiz_question",
            QuizzingConstants.Schema);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired();

            builder.Property(x => x.Position)
                .IsRequired();

            builder.Property(x => x.ValueScore)
                .IsRequired();

            builder.Property(x => x.QuizId)
                .IsRequired();

            builder.Property(x => x.QuestionId)
                .IsRequired();

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

            builder.HasIndex(x => new
            {
                x.QuizId,
                x.Position
            }).IsUnique();

            builder.HasIndex(x => new
            {
                x.QuizId,
                x.QuestionId
            }).IsUnique();

            builder.HasOne<Question>()
                .WithMany()
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}