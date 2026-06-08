using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AgriculturePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FarmSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_Companies_CompanyId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Companies_CompanyId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Companies_CompanyId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_CropCycles_Companies_CompanyId",
                table: "CropCycles");

            migrationBuilder.DropForeignKey(
                name: "FK_Fields_Companies_CompanyId",
                table: "Fields");

            migrationBuilder.DropForeignKey(
                name: "FK_Harvests_Companies_CompanyId",
                table: "Harvests");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Companies_CompanyId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Observations_Companies_CompanyId",
                table: "Observations");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityChecks_Companies_CompanyId",
                table: "QualityChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_SensorReadings_Companies_CompanyId",
                table: "SensorReadings");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Companies_CompanyId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_WeatherData_Companies_CompanyId",
                table: "WeatherData");

            migrationBuilder.DropForeignKey(
                name: "FK_Workers_Companies_CompanyId",
                table: "Workers");

            migrationBuilder.DropForeignKey(
                name: "FK_YieldReports_Companies_CompanyId",
                table: "YieldReports");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "YieldReports",
                newName: "FarmId");

            migrationBuilder.RenameIndex(
                name: "IX_YieldReports_CompanyId_ReportDate",
                table: "YieldReports",
                newName: "IX_YieldReports_FarmId_ReportDate");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Workers",
                newName: "FarmId");

            migrationBuilder.RenameIndex(
                name: "IX_Workers_CompanyId_Role",
                table: "Workers",
                newName: "IX_Workers_FarmId_Role");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "WeatherData",
                newName: "FarmId");

            migrationBuilder.RenameIndex(
                name: "IX_WeatherData_CompanyId",
                table: "WeatherData",
                newName: "IX_WeatherData_FarmId");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Tasks",
                newName: "FarmId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_CompanyId_Status",
                table: "Tasks",
                newName: "IX_Tasks_FarmId_Status");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "SensorReadings",
                newName: "FarmId");

            migrationBuilder.RenameIndex(
                name: "IX_SensorReadings_CompanyId_RecordedAt",
                table: "SensorReadings",
                newName: "IX_SensorReadings_FarmId_RecordedAt");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "QualityChecks",
                newName: "FarmId");

            migrationBuilder.RenameIndex(
                name: "IX_QualityChecks_CompanyId",
                table: "QualityChecks",
                newName: "IX_QualityChecks_FarmId");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Observations",
                newName: "FarmId");

            migrationBuilder.RenameIndex(
                name: "IX_Observations_CompanyId",
                table: "Observations",
                newName: "IX_Observations_FarmId");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Notifications",
                newName: "FarmId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_CompanyId",
                table: "Notifications",
                newName: "IX_Notifications_FarmId");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Harvests",
                newName: "FarmId");

            migrationBuilder.RenameIndex(
                name: "IX_Harvests_CompanyId_HarvestDate",
                table: "Harvests",
                newName: "IX_Harvests_FarmId_HarvestDate");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Fields",
                newName: "FarmId");

            migrationBuilder.RenameIndex(
                name: "IX_Fields_CompanyId_Status",
                table: "Fields",
                newName: "IX_Fields_FarmId_Status");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "CropCycles",
                newName: "FarmId");

            migrationBuilder.RenameIndex(
                name: "IX_CropCycles_CompanyId_Status",
                table: "CropCycles",
                newName: "IX_CropCycles_FarmId_Status");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "AuditLogs",
                newName: "FarmId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_CompanyId_CreatedAt",
                table: "AuditLogs",
                newName: "IX_AuditLogs_FarmId_CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Alerts",
                newName: "FarmId");

            migrationBuilder.RenameIndex(
                name: "IX_Alerts_CompanyId_IsResolved",
                table: "Alerts",
                newName: "IX_Alerts_FarmId_IsResolved");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Admins",
                newName: "FarmId");

            migrationBuilder.RenameIndex(
                name: "IX_Admins_CompanyId",
                table: "Admins",
                newName: "IX_Admins_FarmId");

            migrationBuilder.CreateTable(
                name: "Farms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FarmName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TotalLandHectares = table.Column<decimal>(type: "numeric", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Farms", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Farms_Email",
                table: "Farms",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_Farms_FarmId",
                table: "Admins",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Farms_FarmId",
                table: "Alerts",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Farms_FarmId",
                table: "AuditLogs",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CropCycles_Farms_FarmId",
                table: "CropCycles",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Fields_Farms_FarmId",
                table: "Fields",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Harvests_Farms_FarmId",
                table: "Harvests",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Farms_FarmId",
                table: "Notifications",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Observations_Farms_FarmId",
                table: "Observations",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QualityChecks_Farms_FarmId",
                table: "QualityChecks",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SensorReadings_Farms_FarmId",
                table: "SensorReadings",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Farms_FarmId",
                table: "Tasks",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WeatherData_Farms_FarmId",
                table: "WeatherData",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workers_Farms_FarmId",
                table: "Workers",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YieldReports_Farms_FarmId",
                table: "YieldReports",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_Farms_FarmId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Farms_FarmId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Farms_FarmId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_CropCycles_Farms_FarmId",
                table: "CropCycles");

            migrationBuilder.DropForeignKey(
                name: "FK_Fields_Farms_FarmId",
                table: "Fields");

            migrationBuilder.DropForeignKey(
                name: "FK_Harvests_Farms_FarmId",
                table: "Harvests");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Farms_FarmId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Observations_Farms_FarmId",
                table: "Observations");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityChecks_Farms_FarmId",
                table: "QualityChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_SensorReadings_Farms_FarmId",
                table: "SensorReadings");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Farms_FarmId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_WeatherData_Farms_FarmId",
                table: "WeatherData");

            migrationBuilder.DropForeignKey(
                name: "FK_Workers_Farms_FarmId",
                table: "Workers");

            migrationBuilder.DropForeignKey(
                name: "FK_YieldReports_Farms_FarmId",
                table: "YieldReports");

            migrationBuilder.DropTable(
                name: "Farms");

            migrationBuilder.RenameColumn(
                name: "FarmId",
                table: "YieldReports",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_YieldReports_FarmId_ReportDate",
                table: "YieldReports",
                newName: "IX_YieldReports_CompanyId_ReportDate");

            migrationBuilder.RenameColumn(
                name: "FarmId",
                table: "Workers",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_Workers_FarmId_Role",
                table: "Workers",
                newName: "IX_Workers_CompanyId_Role");

            migrationBuilder.RenameColumn(
                name: "FarmId",
                table: "WeatherData",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_WeatherData_FarmId",
                table: "WeatherData",
                newName: "IX_WeatherData_CompanyId");

            migrationBuilder.RenameColumn(
                name: "FarmId",
                table: "Tasks",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_FarmId_Status",
                table: "Tasks",
                newName: "IX_Tasks_CompanyId_Status");

            migrationBuilder.RenameColumn(
                name: "FarmId",
                table: "SensorReadings",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_SensorReadings_FarmId_RecordedAt",
                table: "SensorReadings",
                newName: "IX_SensorReadings_CompanyId_RecordedAt");

            migrationBuilder.RenameColumn(
                name: "FarmId",
                table: "QualityChecks",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_QualityChecks_FarmId",
                table: "QualityChecks",
                newName: "IX_QualityChecks_CompanyId");

            migrationBuilder.RenameColumn(
                name: "FarmId",
                table: "Observations",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_Observations_FarmId",
                table: "Observations",
                newName: "IX_Observations_CompanyId");

            migrationBuilder.RenameColumn(
                name: "FarmId",
                table: "Notifications",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_FarmId",
                table: "Notifications",
                newName: "IX_Notifications_CompanyId");

            migrationBuilder.RenameColumn(
                name: "FarmId",
                table: "Harvests",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_Harvests_FarmId_HarvestDate",
                table: "Harvests",
                newName: "IX_Harvests_CompanyId_HarvestDate");

            migrationBuilder.RenameColumn(
                name: "FarmId",
                table: "Fields",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_Fields_FarmId_Status",
                table: "Fields",
                newName: "IX_Fields_CompanyId_Status");

            migrationBuilder.RenameColumn(
                name: "FarmId",
                table: "CropCycles",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_CropCycles_FarmId_Status",
                table: "CropCycles",
                newName: "IX_CropCycles_CompanyId_Status");

            migrationBuilder.RenameColumn(
                name: "FarmId",
                table: "AuditLogs",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_FarmId_CreatedAt",
                table: "AuditLogs",
                newName: "IX_AuditLogs_CompanyId_CreatedAt");

            migrationBuilder.RenameColumn(
                name: "FarmId",
                table: "Alerts",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_Alerts_FarmId_IsResolved",
                table: "Alerts",
                newName: "IX_Alerts_CompanyId_IsResolved");

            migrationBuilder.RenameColumn(
                name: "FarmId",
                table: "Admins",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_Admins_FarmId",
                table: "Admins",
                newName: "IX_Admins_CompanyId");

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Email",
                table: "Companies",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_Companies_CompanyId",
                table: "Admins",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Companies_CompanyId",
                table: "Alerts",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Companies_CompanyId",
                table: "AuditLogs",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CropCycles_Companies_CompanyId",
                table: "CropCycles",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Fields_Companies_CompanyId",
                table: "Fields",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Harvests_Companies_CompanyId",
                table: "Harvests",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Companies_CompanyId",
                table: "Notifications",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Observations_Companies_CompanyId",
                table: "Observations",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QualityChecks_Companies_CompanyId",
                table: "QualityChecks",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SensorReadings_Companies_CompanyId",
                table: "SensorReadings",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Companies_CompanyId",
                table: "Tasks",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WeatherData_Companies_CompanyId",
                table: "WeatherData",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workers_Companies_CompanyId",
                table: "Workers",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YieldReports_Companies_CompanyId",
                table: "YieldReports",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
