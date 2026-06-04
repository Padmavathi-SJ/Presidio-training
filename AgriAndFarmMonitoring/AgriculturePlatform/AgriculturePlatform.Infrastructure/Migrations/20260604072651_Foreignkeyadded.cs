using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriculturePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Foreignkeyadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Harvests_Workers_HarvesterId",
                table: "Harvests");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityChecks_Workers_CheckerId",
                table: "QualityChecks");

            migrationBuilder.DropIndex(
                name: "IX_QualityChecks_CheckerId",
                table: "QualityChecks");

            migrationBuilder.DropIndex(
                name: "IX_Harvests_HarvesterId",
                table: "Harvests");

            migrationBuilder.DropColumn(
                name: "CheckerId",
                table: "QualityChecks");

            migrationBuilder.DropColumn(
                name: "HarvesterId",
                table: "Harvests");

            migrationBuilder.CreateIndex(
                name: "IX_QualityChecks_CheckedBy",
                table: "QualityChecks",
                column: "CheckedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Harvests_HarvestedBy",
                table: "Harvests",
                column: "HarvestedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Harvests_Workers_HarvestedBy",
                table: "Harvests",
                column: "HarvestedBy",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QualityChecks_Workers_CheckedBy",
                table: "QualityChecks",
                column: "CheckedBy",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Harvests_Workers_HarvestedBy",
                table: "Harvests");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityChecks_Workers_CheckedBy",
                table: "QualityChecks");

            migrationBuilder.DropIndex(
                name: "IX_QualityChecks_CheckedBy",
                table: "QualityChecks");

            migrationBuilder.DropIndex(
                name: "IX_Harvests_HarvestedBy",
                table: "Harvests");

            migrationBuilder.AddColumn<int>(
                name: "CheckerId",
                table: "QualityChecks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HarvesterId",
                table: "Harvests",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualityChecks_CheckerId",
                table: "QualityChecks",
                column: "CheckerId");

            migrationBuilder.CreateIndex(
                name: "IX_Harvests_HarvesterId",
                table: "Harvests",
                column: "HarvesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Harvests_Workers_HarvesterId",
                table: "Harvests",
                column: "HarvesterId",
                principalTable: "Workers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QualityChecks_Workers_CheckerId",
                table: "QualityChecks",
                column: "CheckerId",
                principalTable: "Workers",
                principalColumn: "Id");
        }
    }
}
