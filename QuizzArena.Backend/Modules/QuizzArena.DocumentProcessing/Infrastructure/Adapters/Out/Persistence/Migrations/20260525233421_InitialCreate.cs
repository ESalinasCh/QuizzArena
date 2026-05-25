using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pgvector;
using QuizzArena.DocumentProcessing.Domain.Enums;

#nullable disable

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "document_processing");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:document_processing.job_status", "pending,processing,completed,failed,discarded")
                .Annotation("Npgsql:Enum:document_processing.source_status", "pending,processing,completed,failed")
                .Annotation("Npgsql:Enum:document_processing.source_type", "video,audio,text")
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "class_source",
                schema: "document_processing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<SourceType>(type: "document_processing.source_type", nullable: false),
                    Status = table.Column<SourceStatus>(type: "document_processing.source_status", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TranscriptUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FileUrl = table.Column<string>(type: "text", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    course_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_d = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_source", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "processing_job",
                schema: "document_processing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<JobStatus>(type: "document_processing.job_status", nullable: false),
                    error_message = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    finished_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processing_job", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "document_chunk",
                schema: "document_processing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChunkOrder = table.Column<int>(type: "integer", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    Embedding = table.Column<Vector>(type: "vector(1024)", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_chunk", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_chunk_class_source_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "document_processing",
                        principalTable: "class_source",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_processing_job",
                schema: "document_processing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessingJobId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_processing_job", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessingJob_ClassSource_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "document_processing",
                        principalTable: "class_source",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_processing_job_processing_job_ProcessingJobId",
                        column: x => x.ProcessingJobId,
                        principalSchema: "document_processing",
                        principalTable: "processing_job",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_chunk_DocumentId",
                schema: "document_processing",
                table: "document_chunk",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_document_processing_job_DocumentId",
                schema: "document_processing",
                table: "document_processing_job",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_document_processing_job_ProcessingJobId",
                schema: "document_processing",
                table: "document_processing_job",
                column: "ProcessingJobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_chunk",
                schema: "document_processing");

            migrationBuilder.DropTable(
                name: "document_processing_job",
                schema: "document_processing");

            migrationBuilder.DropTable(
                name: "class_source",
                schema: "document_processing");

            migrationBuilder.DropTable(
                name: "processing_job",
                schema: "document_processing");
        }
    }
}
