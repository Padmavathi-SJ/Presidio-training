using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AgriculturePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PasswordResetExpires = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admins_Admins_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Admins_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    AdminId = table.Column<int>(type: "integer", nullable: false),
                    FieldName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AreaHectares = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    SoilType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fields_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Fields_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Workers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    AdminId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HireDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workers_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Workers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CropCycles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    AdminId = table.Column<int>(type: "integer", nullable: false),
                    FieldId = table.Column<int>(type: "integer", nullable: false),
                    CropType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PlantingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpectedHarvestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GrowthStage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropCycles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CropCycles_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CropCycles_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CropCycles_Fields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeatherData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    AdminId = table.Column<int>(type: "integer", nullable: false),
                    FieldId = table.Column<int>(type: "integer", nullable: false),
                    Temperature = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    Humidity = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    RainfallMm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    WindSpeed = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    Condition = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeatherData_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeatherData_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeatherData_Fields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    AdminId = table.Column<int>(type: "integer", nullable: true),
                    WorkerId = table.Column<int>(type: "integer", nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EntityId = table.Column<int>(type: "integer", nullable: true),
                    OldValue = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    NewValue = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuditLogs_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    AdminId = table.Column<int>(type: "integer", nullable: true),
                    WorkerId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ActionUrl = table.Column<string>(type: "text", nullable: true),
                    Metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    AdminId = table.Column<int>(type: "integer", nullable: false),
                    FieldId = table.Column<int>(type: "integer", nullable: false),
                    CropCycleId = table.Column<int>(type: "integer", nullable: true),
                    AlertType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alerts_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Alerts_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Alerts_CropCycles_CropCycleId",
                        column: x => x.CropCycleId,
                        principalTable: "CropCycles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Alerts_Fields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Harvests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    AdminId = table.Column<int>(type: "integer", nullable: false),
                    FieldId = table.Column<int>(type: "integer", nullable: false),
                    CropCycleId = table.Column<int>(type: "integer", nullable: false),
                    HarvestedBy = table.Column<int>(type: "integer", nullable: true),
                    HarvestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QuantityKg = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    QualityGrade = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ApprovalStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ApprovedBy = table.Column<int>(type: "integer", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HarvesterId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Harvests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Harvests_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Harvests_Admins_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Harvests_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Harvests_CropCycles_CropCycleId",
                        column: x => x.CropCycleId,
                        principalTable: "CropCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Harvests_Fields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Harvests_Workers_HarvesterId",
                        column: x => x.HarvesterId,
                        principalTable: "Workers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Observations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    AdminId = table.Column<int>(type: "integer", nullable: false),
                    FieldId = table.Column<int>(type: "integer", nullable: false),
                    CropCycleId = table.Column<int>(type: "integer", nullable: true),
                    WorkerId = table.Column<int>(type: "integer", nullable: true),
                    ObservationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CropHealth = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PestDetected = table.Column<bool>(type: "boolean", nullable: false),
                    PestType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Observations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Observations_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Observations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Observations_CropCycles_CropCycleId",
                        column: x => x.CropCycleId,
                        principalTable: "CropCycles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Observations_Fields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Observations_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SensorReadings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    AdminId = table.Column<int>(type: "integer", nullable: false),
                    FieldId = table.Column<int>(type: "integer", nullable: false),
                    CropCycleId = table.Column<int>(type: "integer", nullable: false),
                    SensorType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Value = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SensorReadings_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SensorReadings_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SensorReadings_CropCycles_CropCycleId",
                        column: x => x.CropCycleId,
                        principalTable: "CropCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SensorReadings_Fields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    AdminId = table.Column<int>(type: "integer", nullable: false),
                    WorkerId = table.Column<int>(type: "integer", nullable: false),
                    FieldId = table.Column<int>(type: "integer", nullable: true),
                    CropCycleId = table.Column<int>(type: "integer", nullable: true),
                    TaskName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tasks_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tasks_CropCycles_CropCycleId",
                        column: x => x.CropCycleId,
                        principalTable: "CropCycles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tasks_Fields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Fields",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tasks_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YieldReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    AdminId = table.Column<int>(type: "integer", nullable: false),
                    CropCycleId = table.Column<int>(type: "integer", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReportType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TotalYieldKg = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    YieldPerHectareKg = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    AvgQualityGrade = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YieldReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YieldReports_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YieldReports_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YieldReports_CropCycles_CropCycleId",
                        column: x => x.CropCycleId,
                        principalTable: "CropCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QualityChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    AdminId = table.Column<int>(type: "integer", nullable: false),
                    HarvestId = table.Column<int>(type: "integer", nullable: false),
                    CheckedBy = table.Column<int>(type: "integer", nullable: true),
                    CheckDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MoisturePct = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    DefectPct = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    FinalGrade = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ApprovalStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ApprovedBy = table.Column<int>(type: "integer", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckerId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityChecks_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QualityChecks_Admins_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualityChecks_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QualityChecks_Harvests_HarvestId",
                        column: x => x.HarvestId,
                        principalTable: "Harvests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QualityChecks_Workers_CheckerId",
                        column: x => x.CheckerId,
                        principalTable: "Workers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admins_CompanyId",
                table: "Admins",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Admins_CreatedBy",
                table: "Admins",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Admins_Email",
                table: "Admins",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admins_IsActive",
                table: "Admins",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_AdminId",
                table: "Alerts",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_CompanyId_IsResolved",
                table: "Alerts",
                columns: new[] { "CompanyId", "IsResolved" });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_CropCycleId",
                table: "Alerts",
                column: "CropCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_FieldId_IsResolved",
                table: "Alerts",
                columns: new[] { "FieldId", "IsResolved" });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_Severity",
                table: "Alerts",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_AdminId",
                table: "AuditLogs",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CompanyId_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "CompanyId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_WorkerId",
                table: "AuditLogs",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Email",
                table: "Companies",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CropCycles_AdminId",
                table: "CropCycles",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_CropCycles_CompanyId_Status",
                table: "CropCycles",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CropCycles_FieldId",
                table: "CropCycles",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_CropCycles_PlantingDate",
                table: "CropCycles",
                column: "PlantingDate");

            migrationBuilder.CreateIndex(
                name: "IX_Fields_AdminId",
                table: "Fields",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Fields_CompanyId_Status",
                table: "Fields",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Fields_FieldName",
                table: "Fields",
                column: "FieldName");

            migrationBuilder.CreateIndex(
                name: "IX_Harvests_AdminId",
                table: "Harvests",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Harvests_ApprovalStatus",
                table: "Harvests",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Harvests_ApprovedBy",
                table: "Harvests",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Harvests_CompanyId_HarvestDate",
                table: "Harvests",
                columns: new[] { "CompanyId", "HarvestDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Harvests_CropCycleId",
                table: "Harvests",
                column: "CropCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_Harvests_FieldId",
                table: "Harvests",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_Harvests_HarvesterId",
                table: "Harvests",
                column: "HarvesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AdminId",
                table: "Notifications",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CompanyId",
                table: "Notifications",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_WorkerId_IsRead",
                table: "Notifications",
                columns: new[] { "WorkerId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Observations_AdminId",
                table: "Observations",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Observations_CompanyId",
                table: "Observations",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Observations_CropCycleId",
                table: "Observations",
                column: "CropCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_Observations_FieldId_ObservationDate",
                table: "Observations",
                columns: new[] { "FieldId", "ObservationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Observations_WorkerId",
                table: "Observations",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityChecks_AdminId",
                table: "QualityChecks",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityChecks_ApprovalStatus",
                table: "QualityChecks",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_QualityChecks_ApprovedBy",
                table: "QualityChecks",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_QualityChecks_CheckerId",
                table: "QualityChecks",
                column: "CheckerId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityChecks_CompanyId",
                table: "QualityChecks",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityChecks_HarvestId",
                table: "QualityChecks",
                column: "HarvestId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_AdminId",
                table: "SensorReadings",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_CompanyId_RecordedAt",
                table: "SensorReadings",
                columns: new[] { "CompanyId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_CropCycleId",
                table: "SensorReadings",
                column: "CropCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_FieldId_RecordedAt",
                table: "SensorReadings",
                columns: new[] { "FieldId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_SensorType",
                table: "SensorReadings",
                column: "SensorType");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AdminId",
                table: "Tasks",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CompanyId_Status",
                table: "Tasks",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CropCycleId",
                table: "Tasks",
                column: "CropCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_FieldId",
                table: "Tasks",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Status_DueDate",
                table: "Tasks",
                columns: new[] { "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_WorkerId_Status",
                table: "Tasks",
                columns: new[] { "WorkerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WeatherData_AdminId",
                table: "WeatherData",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherData_CompanyId",
                table: "WeatherData",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherData_FieldId_RecordedAt",
                table: "WeatherData",
                columns: new[] { "FieldId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Workers_AdminId",
                table: "Workers",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Workers_CompanyId_Role",
                table: "Workers",
                columns: new[] { "CompanyId", "Role" });

            migrationBuilder.CreateIndex(
                name: "IX_Workers_Email",
                table: "Workers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workers_IsActive",
                table: "Workers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_YieldReports_AdminId",
                table: "YieldReports",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_YieldReports_CompanyId_ReportDate",
                table: "YieldReports",
                columns: new[] { "CompanyId", "ReportDate" });

            migrationBuilder.CreateIndex(
                name: "IX_YieldReports_CropCycleId",
                table: "YieldReports",
                column: "CropCycleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Observations");

            migrationBuilder.DropTable(
                name: "QualityChecks");

            migrationBuilder.DropTable(
                name: "SensorReadings");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "WeatherData");

            migrationBuilder.DropTable(
                name: "YieldReports");

            migrationBuilder.DropTable(
                name: "Harvests");

            migrationBuilder.DropTable(
                name: "CropCycles");

            migrationBuilder.DropTable(
                name: "Workers");

            migrationBuilder.DropTable(
                name: "Fields");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
