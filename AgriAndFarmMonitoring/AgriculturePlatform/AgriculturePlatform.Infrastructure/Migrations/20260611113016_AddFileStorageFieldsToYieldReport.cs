using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriculturePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFileStorageFieldsToYieldReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExportFormat",
                table: "YieldReports",
                newName: "FileFormat");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "YieldReports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "YieldReports",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "YieldReports");

            migrationBuilder.RenameColumn(
                name: "FileFormat",
                table: "YieldReports",
                newName: "ExportFormat");
        }
    }
}
