using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AgriculturePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class newColumnsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "YieldReports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "YieldReports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "YieldReports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "YieldReports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "YieldReports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Workers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Workers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "Workers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Workers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Workers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "WeatherData",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "WeatherData",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "WeatherData",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "WeatherData",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "WeatherData",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "WeatherData",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "Tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Tasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "SensorReadings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SensorReadings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "SensorReadings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SensorReadings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "SensorReadings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SensorReadings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "SensorReadings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "QualityChecks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "QualityChecks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "QualityChecks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "QualityChecks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "QualityChecks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Observations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Observations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "Observations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Observations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Observations",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Notifications",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Notifications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "Notifications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Notifications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Harvests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Harvests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "Harvests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Harvests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Harvests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Fields",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Fields",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "Fields",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Fields",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Fields",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Farms",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Farms",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "Farms",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Farms",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Farms",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "CropCycles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CropCycles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "CropCycles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CropCycles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "CropCycles",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "AuditLogs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "AuditLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AuditLogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "AuditLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AuditLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AuditLogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "AuditLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Alerts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Alerts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "Alerts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Alerts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Alerts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Admins",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "Admins",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Admins",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Admins",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "WeatherData");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "WeatherData");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "WeatherData");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "WeatherData");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "WeatherData");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "WeatherData");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SensorReadings");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SensorReadings");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SensorReadings");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SensorReadings");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SensorReadings");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SensorReadings");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "QualityChecks");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "QualityChecks");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "QualityChecks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "QualityChecks");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "QualityChecks");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Harvests");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Harvests");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Harvests");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Harvests");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Harvests");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CropCycles");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CropCycles");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CropCycles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CropCycles");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "CropCycles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Admins");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "SensorReadings",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Notifications",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "AuditLogs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
