using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.Users.Domain.Entities;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Configurations;

internal sealed class CourseStudentConfiguration : IEntityTypeConfiguration<CourseStudent>
{
    public void Configure(EntityTypeBuilder<CourseStudent> builder)
    {
        builder.ToTable(
            "course_student",
            schema: UserConstants.Schema
            );

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Deleted).
            HasDefaultValue(false);

        builder.Property(x => x.DeletedAt).
            HasColumnType("timestamptz");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
