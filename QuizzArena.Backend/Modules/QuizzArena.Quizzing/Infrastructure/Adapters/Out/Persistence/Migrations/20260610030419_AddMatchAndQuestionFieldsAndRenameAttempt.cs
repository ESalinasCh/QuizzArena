using System;
using Microsoft.EntityFrameworkCore.Migrations;
using QuizzArena.Quizzing.Domain.Enums;

#nullable disable

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchAndQuestionFieldsAndRenameAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_answer_quiz_attempt_QuizAttemptId",
                schema: "quizzing",
                table: "answer");

            migrationBuilder.DropTable(
                name: "quiz_attempt",
                schema: "quizzing");

            migrationBuilder.DropColumn(
                name: "WasModified",
                schema: "quizzing",
                table: "question");

            migrationBuilder.RenameColumn(
                name: "QuizAttemptId",
                schema: "quizzing",
                table: "answer",
                newName: "MatchAttemptId");

            migrationBuilder.RenameIndex(
                name: "IX_answer_QuizAttemptId",
                schema: "quizzing",
                table: "answer",
                newName: "IX_answer_MatchAttemptId");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:quizzing.match_mode", "single,multiple")
                .Annotation("Npgsql:Enum:quizzing.match_status", "pending,active,expired")
                .Annotation("Npgsql:Enum:quizzing.question_origin", "ai_generated,mixed,manually_created")
                .Annotation("Npgsql:Enum:quizzing.question_status", "draft,verified,disapproved")
                .Annotation("Npgsql:Enum:quizzing.question_type", "multiple_choice,single_choice,true_false")
                .Annotation("Npgsql:Enum:quizzing.quiz_attempt_status", "in_progress,completed,timeout")
                .Annotation("Npgsql:Enum:quizzing.quiz_origin", "ai_generated,manually_created")
                .Annotation("Npgsql:Enum:quizzing.quiz_status", "draft,published,archived")
                .OldAnnotation("Npgsql:Enum:quizzing.match_mode", "single,multiple")
                .OldAnnotation("Npgsql:Enum:quizzing.match_status", "pending,active,expired")
                .OldAnnotation("Npgsql:Enum:quizzing.question_status", "draft,verified,disapproved")
                .OldAnnotation("Npgsql:Enum:quizzing.question_type", "multiple_choice,single_choice,true_false")
                .OldAnnotation("Npgsql:Enum:quizzing.quiz_attempt_status", "in_progress,completed,timeout")
                .OldAnnotation("Npgsql:Enum:quizzing.quiz_status", "draft,published,archived");

            migrationBuilder.AddColumn<QuizOrigin>(
                name: "Origin",
                schema: "quizzing",
                table: "quiz",
                type: "quizzing.quiz_origin",
                nullable: false,
                defaultValue: QuizOrigin.AiGenerated);

            migrationBuilder.AddColumn<QuestionOrigin>(
                name: "Origin",
                schema: "quizzing",
                table: "question",
                type: "quizzing.question_origin",
                nullable: false,
                defaultValue: QuestionOrigin.AiGenerated);

            migrationBuilder.AddColumn<int>(
                name: "AttemptsAmount",
                schema: "quizzing",
                table: "match",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuestionsAmount",
                schema: "quizzing",
                table: "match",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShuffleOptions",
                schema: "quizzing",
                table: "match",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShuffleQuestion",
                schema: "quizzing",
                table: "match",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "match_attempt",
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
                    table.PrimaryKey("PK_match_attempt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_match_attempt_match_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "quizzing",
                        principalTable: "match",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "match_attempt_question",
                schema: "quizzing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ValueScore = table.Column<int>(type: "integer", nullable: true),
                    MatchAttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_match_attempt_question", x => x.Id);
                    table.ForeignKey(
                        name: "FK_match_attempt_question_match_attempt_MatchAttemptId",
                        column: x => x.MatchAttemptId,
                        principalSchema: "quizzing",
                        principalTable: "match_attempt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_match_attempt_question_question_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "quizzing",
                        principalTable: "question",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_match_attempt_MatchId",
                schema: "quizzing",
                table: "match_attempt",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_match_attempt_question_MatchAttemptId",
                schema: "quizzing",
                table: "match_attempt_question",
                column: "MatchAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_match_attempt_question_QuestionId",
                schema: "quizzing",
                table: "match_attempt_question",
                column: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_answer_match_attempt_MatchAttemptId",
                schema: "quizzing",
                table: "answer",
                column: "MatchAttemptId",
                principalSchema: "quizzing",
                principalTable: "match_attempt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_answer_match_attempt_MatchAttemptId",
                schema: "quizzing",
                table: "answer");

            migrationBuilder.DropTable(
                name: "match_attempt_question",
                schema: "quizzing");

            migrationBuilder.DropTable(
                name: "match_attempt",
                schema: "quizzing");

            migrationBuilder.DropColumn(
                name: "Origin",
                schema: "quizzing",
                table: "quiz");

            migrationBuilder.DropColumn(
                name: "Origin",
                schema: "quizzing",
                table: "question");

            migrationBuilder.DropColumn(
                name: "AttemptsAmount",
                schema: "quizzing",
                table: "match");

            migrationBuilder.DropColumn(
                name: "QuestionsAmount",
                schema: "quizzing",
                table: "match");

            migrationBuilder.DropColumn(
                name: "ShuffleOptions",
                schema: "quizzing",
                table: "match");

            migrationBuilder.DropColumn(
                name: "ShuffleQuestion",
                schema: "quizzing",
                table: "match");

            migrationBuilder.RenameColumn(
                name: "MatchAttemptId",
                schema: "quizzing",
                table: "answer",
                newName: "QuizAttemptId");

            migrationBuilder.RenameIndex(
                name: "IX_answer_MatchAttemptId",
                schema: "quizzing",
                table: "answer",
                newName: "IX_answer_QuizAttemptId");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:quizzing.match_mode", "single,multiple")
                .Annotation("Npgsql:Enum:quizzing.match_status", "pending,active,expired")
                .Annotation("Npgsql:Enum:quizzing.question_status", "draft,verified,disapproved")
                .Annotation("Npgsql:Enum:quizzing.question_type", "multiple_choice,single_choice,true_false")
                .Annotation("Npgsql:Enum:quizzing.quiz_attempt_status", "in_progress,completed,timeout")
                .Annotation("Npgsql:Enum:quizzing.quiz_status", "draft,published,archived")
                .OldAnnotation("Npgsql:Enum:quizzing.match_mode", "single,multiple")
                .OldAnnotation("Npgsql:Enum:quizzing.match_status", "pending,active,expired")
                .OldAnnotation("Npgsql:Enum:quizzing.question_origin", "ai_generated,mixed,manually_created")
                .OldAnnotation("Npgsql:Enum:quizzing.question_status", "draft,verified,disapproved")
                .OldAnnotation("Npgsql:Enum:quizzing.question_type", "multiple_choice,single_choice,true_false")
                .OldAnnotation("Npgsql:Enum:quizzing.quiz_attempt_status", "in_progress,completed,timeout")
                .OldAnnotation("Npgsql:Enum:quizzing.quiz_origin", "ai_generated,manually_created")
                .OldAnnotation("Npgsql:Enum:quizzing.quiz_status", "draft,published,archived");

            migrationBuilder.AddColumn<bool>(
                name: "WasModified",
                schema: "quizzing",
                table: "question",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "quiz_attempt",
                schema: "quizzing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EndDateTime = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    JoinedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nickname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    StartDateTime = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    Status = table.Column<QuizAttemptStatus>(type: "quizzing.quiz_attempt_status", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempt_MatchId",
                schema: "quizzing",
                table: "quiz_attempt",
                column: "MatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_answer_quiz_attempt_QuizAttemptId",
                schema: "quizzing",
                table: "answer",
                column: "QuizAttemptId",
                principalSchema: "quizzing",
                principalTable: "quiz_attempt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
