using Microsoft.EntityFrameworkCore;
using QuizzArena.Users.Domain.Entities;
using QuizzArena.Users.Domain.Enums;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence;

internal class UserDbContext : DbContext
{
    public UserDbContext(
        DbContextOptions<UserDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseStudent> CourseStudents => Set<CourseStudent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // user_management schema
        modelBuilder.HasDefaultSchema(UserConstants.Schema);
        modelBuilder.HasPostgresEnum<UserRole>(schema: UserConstants.Schema, name: "user_role");

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(UserDbContext).Assembly
        );
    }
}
