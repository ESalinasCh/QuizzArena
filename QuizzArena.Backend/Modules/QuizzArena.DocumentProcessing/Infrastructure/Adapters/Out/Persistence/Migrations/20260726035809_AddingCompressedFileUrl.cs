using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence.Migrations;

/// <inheritdoc />
public partial class AddingCompressedFileUrl : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "TranscriptUrl",
            schema: "document_processing",
            table: "class_source",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(255)",
            oldMaxLength: 255,
            oldNullable: true);

        migrationBuilder.AddColumn<string>(
            name: "CompressedFileUrl",
            schema: "document_processing",
            table: "class_source",
            type: "text",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CompressedFileUrl",
            schema: "document_processing",
            table: "class_source");

        migrationBuilder.AlterColumn<string>(
            name: "TranscriptUrl",
            schema: "document_processing",
            table: "class_source",
            type: "character varying(255)",
            maxLength: 255,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);
    }
}
