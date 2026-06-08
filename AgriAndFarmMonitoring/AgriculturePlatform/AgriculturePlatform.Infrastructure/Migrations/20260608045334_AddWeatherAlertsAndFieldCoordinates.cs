using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AgriculturePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWeatherAlertsAndFieldCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeatherAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FarmId = table.Column<int>(type: "integer", nullable: false),
                    AdminId = table.Column<int>(type: "integer", nullable: false),
                    FieldId = table.Column<int>(type: "integer", nullable: false),
                    AlertType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Temperature = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: true),
                    WindSpeed = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: true),
                    RainfallMm = table.Column<double>(type: "double precision", precision: 6, scale: 2, nullable: true),
                    IsAcknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedBy = table.Column<int>(type: "integer", nullable: true),
                    AlertTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_WeatherAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeatherAlerts_Admins_AcknowledgedBy",
                        column: x => x.AcknowledgedBy,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeatherAlerts_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeatherAlerts_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeatherAlerts_Fields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeatherAlerts_AcknowledgedBy",
                table: "WeatherAlerts",
                column: "AcknowledgedBy");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherAlerts_AdminId",
                table: "WeatherAlerts",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherAlerts_AlertTime",
                table: "WeatherAlerts",
                column: "AlertTime");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherAlerts_AlertType",
                table: "WeatherAlerts",
                column: "AlertType");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherAlerts_ExpiresAt",
                table: "WeatherAlerts",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherAlerts_FarmId_FieldId",
                table: "WeatherAlerts",
                columns: new[] { "FarmId", "FieldId" });

            migrationBuilder.CreateIndex(
                name: "IX_WeatherAlerts_FieldId",
                table: "WeatherAlerts",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherAlerts_IsAcknowledged",
                table: "WeatherAlerts",
                column: "IsAcknowledged");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherAlerts_Severity",
                table: "WeatherAlerts",
                column: "Severity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeatherAlerts");
        }
    }
}
