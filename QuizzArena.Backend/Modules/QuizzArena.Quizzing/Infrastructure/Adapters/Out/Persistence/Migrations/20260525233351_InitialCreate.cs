using Microsoft.EntityFrameworkCore.Migrations;
using QuizzArena.Quizzing.Domain.Enums;

#nullable disable

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "quizzing");

        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:quizzing.match_mode", "single,multiple")
            .Annotation("Npgsql:Enum:quizzing.match_status", "pending,active,expired")
            .Annotation("Npgsql:Enum:quizzing.question_status", "draft,verified,disapproved")
            .Annotation("Npgsql:Enum:quizzing.question_type", "multiple_choice,single_choice,true_false")
            .Annotation("Npgsql:Enum:quizzing.quiz_attempt_status", "in_progress,completed,timeout")
            .Annotation("Npgsql:Enum:quizzing.quiz_status", "draft,published,archived");

        migrationBuilder.CreateTable(
            name: "question",
            schema: "quizzing",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Content = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Status = table.Column<QuestionStatus>(type: "quizzing.question_status", nullable: false),
                WasModified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                Type = table.Column<QuestionType>(type: "quizzing.question_type", nullable: false),
                Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                ProcessingJobId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_question", x => x.Id));

        migrationBuilder.CreateTable(
            name: "quiz",
            schema: "quizzing",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Status = table.Column<QuizStatus>(type: "quizzing.quiz_status", nullable: false),
                Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_quiz", x => x.Id));

        migrationBuilder.CreateTable(
            name: "option",
            schema: "quizzing",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                Position = table.Column<int>(type: "integer", nullable: false),
                Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                QuestionId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_option", x => x.Id);
                table.ForeignKey(
                    name: "FK_option_question_QuestionId",
                    column: x => x.QuestionId,
                    principalSchema: "quizzing",
                    principalTable: "question",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "match",
            schema: "quizzing",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Status = table.Column<MatchStatus>(type: "quizzing.match_status", nullable: false),
                StartedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                FinishedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                Mode = table.Column<MatchMode>(type: "quizzing.match_mode", nullable: false),
                TimeMinutes = table.Column<int>(type: "integer", nullable: false),
                Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                QuizId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_match", x => x.Id);
                table.ForeignKey(
                    name: "FK_match_quiz_QuizId",
                    column: x => x.QuizId,
                    principalSchema: "quizzing",
                    principalTable: "quiz",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "quiz_question",
            schema: "quizzing",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Position = table.Column<int>(type: "integer", nullable: false),
                ValueScore = table.Column<int>(type: "integer", nullable: false),
                Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                QuizId = table.Column<Guid>(type: "uuid", nullable: false),
                QuestionId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_quiz_question", x => x.Id);
                table.ForeignKey(
                    name: "FK_quiz_question_question_QuestionId",
                    column: x => x.QuestionId,
                    principalSchema: "quizzing",
                    principalTable: "question",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_quiz_question_quiz_QuizId",
                    column: x => x.QuizId,
                    principalSchema: "quizzing",
                    principalTable: "quiz",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "quiz_attempt",
            schema: "quizzing",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                StartDateTime = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                EndDateTime = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                JoinedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                Nickname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Status = table.Column<QuizAttemptStatus>(type: "quizzing.quiz_attempt_status", nullable: false),
                Score = table.Column<int>(type: "integer", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                MatchId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_quiz_attempt", x => x.Id);
                table.ForeignKey(
                    name: "FK_quiz_attempt_match_MatchId",
                    column: x => x.MatchId,
                    principalSchema: "quizzing",
                    principalTable: "match",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "answer",
            schema: "quizzing",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                AnsweredAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                TimeMs = table.Column<int>(type: "integer", nullable: false),
                OptionId = table.Column<Guid>(type: "uuid", nullable: false),
                QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                QuizAttemptId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_answer", x => x.Id);
                table.ForeignKey(
                    name: "FK_answer_option_OptionId",
                    column: x => x.OptionId,
                    principalSchema: "quizzing",
                    principalTable: "option",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_answer_question_QuestionId",
                    column: x => x.QuestionId,
                    principalSchema: "quizzing",
                    principalTable: "question",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_answer_quiz_attempt_QuizAttemptId",
                    column: x => x.QuizAttemptId,
                    principalSchema: "quizzing",
                    principalTable: "quiz_attempt",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_answer_OptionId",
            schema: "quizzing",
            table: "answer",
            column: "OptionId");

        migrationBuilder.CreateIndex(
            name: "IX_answer_QuestionId",
            schema: "quizzing",
            table: "answer",
            column: "QuestionId");

        migrationBuilder.CreateIndex(
            name: "IX_answer_QuizAttemptId",
            schema: "quizzing",
            table: "answer",
            column: "QuizAttemptId");

        migrationBuilder.CreateIndex(
            name: "IX_match_Code",
            schema: "quizzing",
            table: "match",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_match_QuizId",
            schema: "quizzing",
            table: "match",
            column: "QuizId");

        migrationBuilder.CreateIndex(
            name: "IX_option_QuestionId_Position",
            schema: "quizzing",
            table: "option",
            columns: new[] { "QuestionId", "Position" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_question_Deleted",
            schema: "quizzing",
            table: "question",
            column: "Deleted");

        migrationBuilder.CreateIndex(
            name: "IX_question_Status",
            schema: "quizzing",
            table: "question",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_question_Type",
            schema: "quizzing",
            table: "question",
            column: "Type");

        migrationBuilder.CreateIndex(
            name: "IX_quiz_attempt_MatchId",
            schema: "quizzing",
            table: "quiz_attempt",
            column: "MatchId");

        migrationBuilder.CreateIndex(
            name: "IX_quiz_question_QuestionId",
            schema: "quizzing",
            table: "quiz_question",
            column: "QuestionId");

        migrationBuilder.CreateIndex(
            name: "IX_quiz_question_QuizId_Position",
            schema: "quizzing",
            table: "quiz_question",
            columns: new[] { "QuizId", "Position" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_quiz_question_QuizId_QuestionId",
            schema: "quizzing",
            table: "quiz_question",
            columns: new[] { "QuizId", "QuestionId" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "answer",
            schema: "quizzing");

        migrationBuilder.DropTable(
            name: "quiz_question",
            schema: "quizzing");

        migrationBuilder.DropTable(
            name: "option",
            schema: "quizzing");

        migrationBuilder.DropTable(
            name: "quiz_attempt",
            schema: "quizzing");

        migrationBuilder.DropTable(
            name: "question",
            schema: "quizzing");

        migrationBuilder.DropTable(
            name: "match",
            schema: "quizzing");

        migrationBuilder.DropTable(
            name: "quiz",
            schema: "quizzing");
    }
}
