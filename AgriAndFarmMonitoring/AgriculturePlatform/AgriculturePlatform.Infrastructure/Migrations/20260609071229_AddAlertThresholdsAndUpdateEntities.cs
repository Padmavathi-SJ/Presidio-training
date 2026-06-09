using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AgriculturePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertThresholdsAndUpdateEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_CropCycles_CropCycleId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_SensorReadings_SensorReadingId",
                table: "Alerts");

            migrationBuilder.AlterColumn<decimal>(
                name: "ThresholdValue",
                table: "Alerts",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SensorValue",
                table: "Alerts",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Alerts",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "AlertThresholds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FarmId = table.Column<int>(type: "integer", nullable: false),
                    AdminId = table.Column<int>(type: "integer", nullable: false),
                    CropType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GrowthStage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SensorType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MinValue = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxValue = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    NotificationEmails = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertThresholds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertThresholds_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlertThresholds_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_AlertType",
                table: "Alerts",
                column: "AlertType");

            migrationBuilder.CreateIndex(
                name: "IX_AlertThresholds_AdminId",
                table: "AlertThresholds",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertThresholds_FarmId_CropType_GrowthStage_SensorType",
                table: "AlertThresholds",
                columns: new[] { "FarmId", "CropType", "GrowthStage", "SensorType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlertThresholds_IsActive",
                table: "AlertThresholds",
                column: "IsActive");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_CropCycles_CropCycleId",
                table: "Alerts",
                column: "CropCycleId",
                principalTable: "CropCycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_SensorReadings_SensorReadingId",
                table: "Alerts",
                column: "SensorReadingId",
                principalTable: "SensorReadings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_CropCycles_CropCycleId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_SensorReadings_SensorReadingId",
                table: "Alerts");

            migrationBuilder.DropTable(
                name: "AlertThresholds");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_AlertType",
                table: "Alerts");

            migrationBuilder.AlterColumn<decimal>(
                name: "ThresholdValue",
                table: "Alerts",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SensorValue",
                table: "Alerts",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Alerts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_CropCycles_CropCycleId",
                table: "Alerts",
                column: "CropCycleId",
                principalTable: "CropCycles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_SensorReadings_SensorReadingId",
                table: "Alerts",
                column: "SensorReadingId",
                principalTable: "SensorReadings",
                principalColumn: "Id");
        }
    }
}
