using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriculturePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCropCycleAutoUpdateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActualHarvestDate",
                table: "CropCycles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoUpdateGrowthStage",
                table: "CropCycles",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStageUpdate",
                table: "CropCycles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousGrowthStage",
                table: "CropCycles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CropCycles_AutoUpdateGrowthStage",
                table: "CropCycles",
                column: "AutoUpdateGrowthStage");

            migrationBuilder.CreateIndex(
                name: "IX_CropCycles_ExpectedHarvestDate",
                table: "CropCycles",
                column: "ExpectedHarvestDate");

            migrationBuilder.CreateIndex(
                name: "IX_CropCycles_GrowthStage",
                table: "CropCycles",
                column: "GrowthStage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CropCycles_AutoUpdateGrowthStage",
                table: "CropCycles");

            migrationBuilder.DropIndex(
                name: "IX_CropCycles_ExpectedHarvestDate",
                table: "CropCycles");

            migrationBuilder.DropIndex(
                name: "IX_CropCycles_GrowthStage",
                table: "CropCycles");

            migrationBuilder.DropColumn(
                name: "ActualHarvestDate",
                table: "CropCycles");

            migrationBuilder.DropColumn(
                name: "AutoUpdateGrowthStage",
                table: "CropCycles");

            migrationBuilder.DropColumn(
                name: "LastStageUpdate",
                table: "CropCycles");

            migrationBuilder.DropColumn(
                name: "PreviousGrowthStage",
                table: "CropCycles");
        }
    }
}
