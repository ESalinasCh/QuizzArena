using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.Users.Domain.Entities;
using QuizzArena.Users.Infraestructure.Adapters.Out.Persistence;
using Users.Domain.Entities;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Configurations
{
    internal class CourseStudentConfiguration : IEntityTypeConfiguration<CourseStudent>
    {
        public void Configure(EntityTypeBuilder<CourseStudent> builder)
        {
            builder.ToTable(
                "course_students",
                schema: UserConstants.Schema
                );

            builder.HasKey(x => x.Id);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .HasConstraintName("FK_Course_User_StudentId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Course>()
                .WithMany()
                .HasForeignKey(x => x.CourseId)
                .HasConstraintName("FK_User_Course_CourseId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
