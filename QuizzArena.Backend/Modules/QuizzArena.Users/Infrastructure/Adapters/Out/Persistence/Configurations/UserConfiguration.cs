using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizzArena.Users.Infraestructure.Adapters.Out.Persistence;
using Users.Domain.Entities;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Configurations
{
    internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable(
                "user",
                schema: UserConstants.Schema
                );

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnType("uuid");

            builder.Property(x => x.UserName)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.ExternalProvider)
                .IsRequired();

            builder.Property(x => x.Deleted)
                .HasDefaultValue(false);

            builder.Property(x => x.Role)
                .HasColumnType($"{UserConstants.Schema}.user_role");

            builder.Property(x => x.AvatarUrl)
                .HasColumnName("avatar_url")
                .HasColumnType("text");

            builder.Property(x => x.ProviderId)
                .HasColumnName("provider_id")
                .HasMaxLength(255);

            builder.Property(x => x.CreatedAt)
                .HasColumnType("timestamptz");

            builder.Property(x => x.UpdatedAt)
                .HasColumnType("timestamptz");

            builder.Property(x => x.DeletedAt)
                .HasColumnType("timestamptz");

        }

    }
}