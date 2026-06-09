using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AgriculturePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSensorReadingsAndAlertsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "SensorReadings",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "SensorReadingId",
                table: "Alerts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SensorValue",
                table: "Alerts",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ThresholdValue",
                table: "Alerts",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_SensorReadingId",
                table: "Alerts",
                column: "SensorReadingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_SensorReadings_SensorReadingId",
                table: "Alerts",
                column: "SensorReadingId",
                principalTable: "SensorReadings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_SensorReadings_SensorReadingId",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_SensorReadingId",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "SensorReadingId",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "SensorValue",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "ThresholdValue",
                table: "Alerts");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "SensorReadings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
