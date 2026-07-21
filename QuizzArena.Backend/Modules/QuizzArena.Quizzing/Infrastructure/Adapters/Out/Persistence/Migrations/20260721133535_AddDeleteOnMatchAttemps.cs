using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Migrations;

/// <inheritdoc />
public partial class AddDeleteOnMatchAttemps : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "Deleted",
            schema: "quizzing",
            table: "match_attempt",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "DeletedAt",
            schema: "quizzing",
            table: "match_attempt",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "UpdatedAt",
            schema: "quizzing",
            table: "match_attempt",
            type: "timestamp with time zone",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Deleted",
            schema: "quizzing",
            table: "match_attempt");

        migrationBuilder.DropColumn(
            name: "DeletedAt",
            schema: "quizzing",
            table: "match_attempt");

        migrationBuilder.DropColumn(
            name: "UpdatedAt",
            schema: "quizzing",
            table: "match_attempt");
    }
}
