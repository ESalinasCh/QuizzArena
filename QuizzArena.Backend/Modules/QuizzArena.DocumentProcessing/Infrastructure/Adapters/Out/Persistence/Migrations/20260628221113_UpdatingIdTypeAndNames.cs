using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence.Migrations;

/// <inheritdoc />
public partial class UpdatingIdTypeAndNames : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "document_processing_job",
            schema: "document_processing");

        migrationBuilder.CreateTable(
            name: "document_processing_job",
            schema: "document_processing",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
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

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "document_processing_job",
            schema: "document_processing");
    }
}
