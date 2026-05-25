using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Configurations
{
    internal sealed class OptionConfiguration : IEntityTypeConfiguration<Option>
    {
        public void Configure(EntityTypeBuilder<Option> builder)
        {
            builder.ToTable(
               "option",
               QuizzingConstants.Schema);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Description)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.IsCorrect)
                .IsRequired();

            builder.Property(x => x.Position)
                .IsRequired();

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

            builder.HasIndex(x => new
            {
                x.QuestionId,
                x.Position
            }).IsUnique();

        }
    }
}