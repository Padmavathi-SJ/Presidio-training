using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriculturePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddYieldReportsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YieldReports_Admins_AdminId",
                table: "YieldReports");

            migrationBuilder.DropForeignKey(
                name: "FK_YieldReports_CropCycles_CropCycleId",
                table: "YieldReports");

            migrationBuilder.DropIndex(
                name: "IX_YieldReports_FarmId_ReportDate",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "YieldPerHectareKg",
                table: "YieldReports");

            migrationBuilder.RenameColumn(
                name: "ReportDate",
                table: "YieldReports",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "AvgQualityGrade",
                table: "YieldReports",
                newName: "AverageQualityGrade");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalYieldKg",
                table: "YieldReports",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,2)",
                oldPrecision: 12,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReportType",
                table: "YieldReports",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CropCycleId",
                table: "YieldReports",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "AdminId1",
                table: "YieldReports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AveragePricePerKg",
                table: "YieldReports",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageYieldPerHectare",
                table: "YieldReports",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CropTypeBreakdownJson",
                table: "YieldReports",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "YieldReports",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ExportFormat",
                table: "YieldReports",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExportedAt",
                table: "YieldReports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExportedBy",
                table: "YieldReports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FieldBreakdownJson",
                table: "YieldReports",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FieldId",
                table: "YieldReports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "YieldReports",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsScheduled",
                table: "YieldReports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastGeneratedAt",
                table: "YieldReports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MonthlyTrendJson",
                table: "YieldReports",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextScheduledRun",
                table: "YieldReports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PassRate",
                table: "YieldReports",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "QualityDistributionJson",
                table: "YieldReports",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RejectionRate",
                table: "YieldReports",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ReportName",
                table: "YieldReports",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ScheduleCron",
                table: "YieldReports",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalHarvests",
                table: "YieldReports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalValue",
                table: "YieldReports",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_YieldReports_AdminId1",
                table: "YieldReports",
                column: "AdminId1");

            migrationBuilder.CreateIndex(
                name: "IX_YieldReports_CreatedAt",
                table: "YieldReports",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_YieldReports_ExportedBy",
                table: "YieldReports",
                column: "ExportedBy");

            migrationBuilder.CreateIndex(
                name: "IX_YieldReports_Farm_DateRange",
                table: "YieldReports",
                columns: new[] { "FarmId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_YieldReports_FieldId",
                table: "YieldReports",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_YieldReports_IsScheduled",
                table: "YieldReports",
                column: "IsScheduled");

            migrationBuilder.CreateIndex(
                name: "IX_YieldReports_NextScheduledRun",
                table: "YieldReports",
                column: "NextScheduledRun");

            migrationBuilder.CreateIndex(
                name: "IX_YieldReports_ReportType",
                table: "YieldReports",
                column: "ReportType");

            migrationBuilder.AddForeignKey(
                name: "FK_YieldReports_Admins_AdminId",
                table: "YieldReports",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_YieldReports_Admins_AdminId1",
                table: "YieldReports",
                column: "AdminId1",
                principalTable: "Admins",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YieldReports_Admins_ExportedBy",
                table: "YieldReports",
                column: "ExportedBy",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_YieldReports_CropCycles_CropCycleId",
                table: "YieldReports",
                column: "CropCycleId",
                principalTable: "CropCycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_YieldReports_Fields_FieldId",
                table: "YieldReports",
                column: "FieldId",
                principalTable: "Fields",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YieldReports_Admins_AdminId",
                table: "YieldReports");

            migrationBuilder.DropForeignKey(
                name: "FK_YieldReports_Admins_AdminId1",
                table: "YieldReports");

            migrationBuilder.DropForeignKey(
                name: "FK_YieldReports_Admins_ExportedBy",
                table: "YieldReports");

            migrationBuilder.DropForeignKey(
                name: "FK_YieldReports_CropCycles_CropCycleId",
                table: "YieldReports");

            migrationBuilder.DropForeignKey(
                name: "FK_YieldReports_Fields_FieldId",
                table: "YieldReports");

            migrationBuilder.DropIndex(
                name: "IX_YieldReports_AdminId1",
                table: "YieldReports");

            migrationBuilder.DropIndex(
                name: "IX_YieldReports_CreatedAt",
                table: "YieldReports");

            migrationBuilder.DropIndex(
                name: "IX_YieldReports_ExportedBy",
                table: "YieldReports");

            migrationBuilder.DropIndex(
                name: "IX_YieldReports_Farm_DateRange",
                table: "YieldReports");

            migrationBuilder.DropIndex(
                name: "IX_YieldReports_FieldId",
                table: "YieldReports");

            migrationBuilder.DropIndex(
                name: "IX_YieldReports_IsScheduled",
                table: "YieldReports");

            migrationBuilder.DropIndex(
                name: "IX_YieldReports_NextScheduledRun",
                table: "YieldReports");

            migrationBuilder.DropIndex(
                name: "IX_YieldReports_ReportType",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "AdminId1",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "AveragePricePerKg",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "AverageYieldPerHectare",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "CropTypeBreakdownJson",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "ExportFormat",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "ExportedAt",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "ExportedBy",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "FieldBreakdownJson",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "FieldId",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "IsScheduled",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "LastGeneratedAt",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "MonthlyTrendJson",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "NextScheduledRun",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "PassRate",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "QualityDistributionJson",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "RejectionRate",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "ReportName",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "ScheduleCron",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "TotalHarvests",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "TotalValue",
                table: "YieldReports");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "YieldReports",
                newName: "ReportDate");

            migrationBuilder.RenameColumn(
                name: "AverageQualityGrade",
                table: "YieldReports",
                newName: "AvgQualityGrade");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalYieldKg",
                table: "YieldReports",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,2)",
                oldPrecision: 12,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "ReportType",
                table: "YieldReports",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "CropCycleId",
                table: "YieldReports",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "YieldPerHectareKg",
                table: "YieldReports",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_YieldReports_FarmId_ReportDate",
                table: "YieldReports",
                columns: new[] { "FarmId", "ReportDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_YieldReports_Admins_AdminId",
                table: "YieldReports",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YieldReports_CropCycles_CropCycleId",
                table: "YieldReports",
                column: "CropCycleId",
                principalTable: "CropCycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
