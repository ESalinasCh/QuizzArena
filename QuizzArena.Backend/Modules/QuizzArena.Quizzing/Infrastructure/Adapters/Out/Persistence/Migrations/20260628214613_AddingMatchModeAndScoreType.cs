using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Migrations;

/// <inheritdoc />
public partial class AddingMatchModeAndScoreType : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:quizzing.match_mode", "single,multiple,exam")
            .Annotation("Npgsql:Enum:quizzing.match_status", "pending,active,expired")
            .Annotation("Npgsql:Enum:quizzing.question_origin", "ai_generated,mixed,manually_created")
            .Annotation("Npgsql:Enum:quizzing.question_status", "draft,verified,disapproved")
            .Annotation("Npgsql:Enum:quizzing.question_type", "multiple_choice,single_choice,true_false")
            .Annotation("Npgsql:Enum:quizzing.quiz_attempt_status", "in_progress,completed,timeout")
            .Annotation("Npgsql:Enum:quizzing.quiz_origin", "ai_generated,manually_created")
            .Annotation("Npgsql:Enum:quizzing.quiz_status", "draft,published,archived")
            .OldAnnotation("Npgsql:Enum:quizzing.match_mode", "single,multiple")
            .OldAnnotation("Npgsql:Enum:quizzing.match_status", "pending,active,expired")
            .OldAnnotation("Npgsql:Enum:quizzing.question_origin", "ai_generated,mixed,manually_created")
            .OldAnnotation("Npgsql:Enum:quizzing.question_status", "draft,verified,disapproved")
            .OldAnnotation("Npgsql:Enum:quizzing.question_type", "multiple_choice,single_choice,true_false")
            .OldAnnotation("Npgsql:Enum:quizzing.quiz_attempt_status", "in_progress,completed,timeout")
            .OldAnnotation("Npgsql:Enum:quizzing.quiz_origin", "ai_generated,manually_created")
            .OldAnnotation("Npgsql:Enum:quizzing.quiz_status", "draft,published,archived");

        migrationBuilder.AlterColumn<decimal>(
            name: "ValueScore",
            schema: "quizzing",
            table: "quiz_question",
            type: "numeric(5,2)",
            precision: 5,
            scale: 2,
            nullable: false,
            oldClrType: typeof(int),
            oldType: "integer");

        migrationBuilder.AlterColumn<decimal>(
            name: "ValueScore",
            schema: "quizzing",
            table: "match_attempt_question",
            type: "numeric(5,2)",
            precision: 5,
            scale: 2,
            nullable: true,
            oldClrType: typeof(int),
            oldType: "integer",
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "Score",
            schema: "quizzing",
            table: "match_attempt",
            type: "numeric(5,2)",
            precision: 5,
            scale: 2,
            nullable: false,
            oldClrType: typeof(int),
            oldType: "integer");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:quizzing.match_mode", "single,multiple")
            .Annotation("Npgsql:Enum:quizzing.match_status", "pending,active,expired")
            .Annotation("Npgsql:Enum:quizzing.question_origin", "ai_generated,mixed,manually_created")
            .Annotation("Npgsql:Enum:quizzing.question_status", "draft,verified,disapproved")
            .Annotation("Npgsql:Enum:quizzing.question_type", "multiple_choice,single_choice,true_false")
            .Annotation("Npgsql:Enum:quizzing.quiz_attempt_status", "in_progress,completed,timeout")
            .Annotation("Npgsql:Enum:quizzing.quiz_origin", "ai_generated,manually_created")
            .Annotation("Npgsql:Enum:quizzing.quiz_status", "draft,published,archived")
            .OldAnnotation("Npgsql:Enum:quizzing.match_mode", "single,multiple,exam")
            .OldAnnotation("Npgsql:Enum:quizzing.match_status", "pending,active,expired")
            .OldAnnotation("Npgsql:Enum:quizzing.question_origin", "ai_generated,mixed,manually_created")
            .OldAnnotation("Npgsql:Enum:quizzing.question_status", "draft,verified,disapproved")
            .OldAnnotation("Npgsql:Enum:quizzing.question_type", "multiple_choice,single_choice,true_false")
            .OldAnnotation("Npgsql:Enum:quizzing.quiz_attempt_status", "in_progress,completed,timeout")
            .OldAnnotation("Npgsql:Enum:quizzing.quiz_origin", "ai_generated,manually_created")
            .OldAnnotation("Npgsql:Enum:quizzing.quiz_status", "draft,published,archived");

        migrationBuilder.AlterColumn<int>(
            name: "ValueScore",
            schema: "quizzing",
            table: "quiz_question",
            type: "integer",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "numeric(5,2)",
            oldPrecision: 5,
            oldScale: 2);

        migrationBuilder.AlterColumn<int>(
            name: "ValueScore",
            schema: "quizzing",
            table: "match_attempt_question",
            type: "integer",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(5,2)",
            oldPrecision: 5,
            oldScale: 2,
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "Score",
            schema: "quizzing",
            table: "match_attempt",
            type: "integer",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "numeric(5,2)",
            oldPrecision: 5,
            oldScale: 2);
    }
}
