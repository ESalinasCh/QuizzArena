using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.Users.Domain.Entities;
using QuizzArena.Users.Infraestructure.Adapters.Out.Persistence;
using Users.Domain.Entities;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Configurations
{
    internal sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.ToTable(
                "course",
                schema: UserConstants.Schema
                );

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(300);

            builder.Property(x => x.Deleted)
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .HasColumnType("timestamptz");

            builder.Property(x => x.UpdatedAt)
                .HasColumnType("timestamptz");

            builder.Property(x => x.DeletedAt)
                .HasColumnType("timestamptz");

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.TeacherId)
                .HasConstraintName("FK_Course_User_TeacherId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.CourseStudents).WithOne().HasForeignKey(x => x.CourseId);
        }
    }
}