using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.Users.Infraestructure.Adapters.Out.Persistence;
using Users.Domain.Entities;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable(
                "users",
                schema: UserConstants.Schema
                );

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnType("uuid");

            builder.Property(x => x.UserName)
                .HasColumnName("user_name")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.ExternalProvider)
                .HasColumnName("external_provider")
                .IsRequired();

            builder.Property(x => x.Deleted)
                .HasColumnName("deleted")
                .HasDefaultValue(false);

            builder.Property(x => x.Role)
                .HasColumnName("role")
                .HasColumnType($"{UserConstants.Schema}.user_role");

            builder.Property(x => x.AvatarUrl)
                .HasColumnName("avatar_url")
                .HasColumnType("text");

            builder.Property(x => x.ProviderId)
                .HasColumnName("provider_id")
                .HasMaxLength(255);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamptz");

            builder.Property(x => x.DeletedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamptz");
        }

    }
}
