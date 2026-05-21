using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.Users.Domain.Entities;
using QuizzArena.Users.Infraestructure.Adapters.Out.Persistence;
using Users.Domain.Entities;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Configurations
{
    internal class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.ToTable(
                "courses",
                schema: UserConstants.Schema
                );

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(300);

            builder.Property(x => x.Deleted)
                .HasColumnName("deleted")
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamptz");

            builder.Property(x => x.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("timestamptz");

            // FK -> User
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.TeacherId)
                .HasConstraintName("FK_Course_User_TeacherId")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
