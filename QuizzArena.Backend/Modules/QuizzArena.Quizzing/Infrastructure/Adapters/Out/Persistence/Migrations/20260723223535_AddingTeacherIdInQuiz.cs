using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Migrations;

/// <inheritdoc />
public partial class AddingTeacherIdInQuiz : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "TeacherId",
            schema: "quizzing",
            table: "quiz",
            type: "uuid",
            nullable: true);

        migrationBuilder.Sql("""
    UPDATE quizzing.quiz
    SET "TeacherId" = '959d0300-4473-4198-b551-6c1c6fb214dc';
""");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TeacherId",
            schema: "quizzing",
            table: "quiz");
    }
}
