using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriculturePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWeatherAlertResolutionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResolutionNotes",
                table: "WeatherAlerts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResolvedByWorkerId",
                table: "WeatherAlerts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeatherAlerts_ResolvedByWorkerId",
                table: "WeatherAlerts",
                column: "ResolvedByWorkerId");

            migrationBuilder.AddForeignKey(
                name: "FK_WeatherAlerts_Workers_ResolvedByWorkerId",
                table: "WeatherAlerts",
                column: "ResolvedByWorkerId",
                principalTable: "Workers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WeatherAlerts_Workers_ResolvedByWorkerId",
                table: "WeatherAlerts");

            migrationBuilder.DropIndex(
                name: "IX_WeatherAlerts_ResolvedByWorkerId",
                table: "WeatherAlerts");

            migrationBuilder.DropColumn(
                name: "ResolutionNotes",
                table: "WeatherAlerts");

            migrationBuilder.DropColumn(
                name: "ResolvedByWorkerId",
                table: "WeatherAlerts");
        }
    }
}
