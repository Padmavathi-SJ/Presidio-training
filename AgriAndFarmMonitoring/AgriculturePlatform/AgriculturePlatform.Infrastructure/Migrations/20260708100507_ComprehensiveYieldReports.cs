using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriculturePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ComprehensiveYieldReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ObservationSummaryJson",
                table: "YieldReports",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SensorStatisticsJson",
                table: "YieldReports",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaskSummaryJson",
                table: "YieldReports",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WeatherStatisticsJson",
                table: "YieldReports",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObservationSummaryJson",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "SensorStatisticsJson",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "TaskSummaryJson",
                table: "YieldReports");

            migrationBuilder.DropColumn(
                name: "WeatherStatisticsJson",
                table: "YieldReports");
        }
    }
}
