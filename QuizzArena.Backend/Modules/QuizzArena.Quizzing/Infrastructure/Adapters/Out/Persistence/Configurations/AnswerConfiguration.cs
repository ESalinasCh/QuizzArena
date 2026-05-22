using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Configurations
{
    internal class AnswerConfiguration : IEntityTypeConfiguration<Answer>
    {
        public void Configure(EntityTypeBuilder<Answer> builder)
        {
            builder.ToTable("answer", QuizzingConstants.Schema);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AnsweredAt)
                .HasColumnType("timestamptz").IsRequired();

            builder.Property(x => x.IsCorrect).IsRequired();
            builder.Property(x => x.TimeMs).IsRequired();

            builder.HasOne<Option>()
                .WithMany()
                .HasForeignKey(x => x.OptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Question>()
                .WithMany()
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<QuizAttempt>()
                .WithMany()
                .HasForeignKey(x => x.QuizAttemptId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
