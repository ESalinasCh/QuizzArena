
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Configurations;

internal sealed class SelectedOptionConfiguration : IEntityTypeConfiguration<SelectedOption>
{
    public void Configure(EntityTypeBuilder<SelectedOption> builder)
    {
        builder.ToTable(
        "selected_option",
        QuizzingConstants.Schema);

        builder.HasKey(x => x.Id);

        builder.HasOne<Option>()
          .WithMany()
          .HasForeignKey(x => x.OptionId)
          .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Answer>()
          .WithMany(x => x.SelectedOptions)
          .HasForeignKey(x => x.AnswerId)
          .OnDelete(DeleteBehavior.Restrict);
    }
}
