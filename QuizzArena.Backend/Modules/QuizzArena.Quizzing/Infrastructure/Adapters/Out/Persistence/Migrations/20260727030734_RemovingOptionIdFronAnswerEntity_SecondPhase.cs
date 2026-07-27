using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Migrations;

/// <inheritdoc />
public partial class RemovingOptionIdFronAnswerEntity_SecondPhase : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_answer_option_OptionId",
            schema: "quizzing",
            table: "answer");

        migrationBuilder.DropIndex(
            name: "IX_answer_OptionId",
            schema: "quizzing",
            table: "answer");

        migrationBuilder.DropColumn(
            name: "OptionId",
            schema: "quizzing",
            table: "answer");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "OptionId",
            schema: "quizzing",
            table: "answer",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.CreateIndex(
            name: "IX_answer_OptionId",
            schema: "quizzing",
            table: "answer",
            column: "OptionId");

        migrationBuilder.AddForeignKey(
            name: "FK_answer_option_OptionId",
            schema: "quizzing",
            table: "answer",
            column: "OptionId",
            principalSchema: "quizzing",
            principalTable: "option",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}
