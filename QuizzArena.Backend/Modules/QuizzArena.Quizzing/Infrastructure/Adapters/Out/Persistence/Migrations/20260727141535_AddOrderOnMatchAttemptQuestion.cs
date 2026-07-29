using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Migrations;

/// <inheritdoc />
public partial class AddOrderOnMatchAttemptQuestion : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Order",
            schema: "quizzing",
            table: "match_attempt_question",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Order",
            schema: "quizzing",
            table: "match_attempt_question");
    }
}
