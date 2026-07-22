using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Migrations;

/// <inheritdoc />
public partial class AddingSelectedOptionsEntity : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "selected_option",
            schema: "quizzing",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OptionId = table.Column<Guid>(type: "uuid", nullable: false),
                AnswerId = table.Column<Guid>(type: "uuid", nullable: false),
                IsCorrect = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_selected_option", x => x.Id);
                table.ForeignKey(
                    name: "FK_selected_option_answer_AnswerId",
                    column: x => x.AnswerId,
                    principalSchema: "quizzing",
                    principalTable: "answer",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_selected_option_option_OptionId",
                    column: x => x.OptionId,
                    principalSchema: "quizzing",
                    principalTable: "option",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_selected_option_AnswerId",
            schema: "quizzing",
            table: "selected_option",
            column: "AnswerId");

        migrationBuilder.CreateIndex(
            name: "IX_selected_option_OptionId",
            schema: "quizzing",
            table: "selected_option",
            column: "OptionId");


        migrationBuilder.Sql("""
            INSERT INTO quizzing.selected_option (
                "Id",
                "OptionId",
                "AnswerId",
                "IsCorrect"
            )
            SELECT
                gen_random_uuid(),
                a."OptionId",
                a."Id",
                a."IsCorrect"
            FROM quizzing.answer a
            WHERE a."OptionId" IS NOT NULL
              AND NOT EXISTS (
                    SELECT 1
                    FROM quizzing.selected_option so
                    WHERE so."AnswerId" = a."Id"
              );
        """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "selected_option",
            schema: "quizzing");
    }
}
