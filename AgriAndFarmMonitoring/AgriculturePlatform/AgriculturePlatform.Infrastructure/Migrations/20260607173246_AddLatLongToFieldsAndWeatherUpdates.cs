using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriculturePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLatLongToFieldsAndWeatherUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "WindSpeed",
                table: "WeatherData",
                type: "double precision",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Temperature",
                table: "WeatherData",
                type: "double precision",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "RainfallMm",
                table: "WeatherData",
                type: "double precision",
                precision: 6,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(6,2)",
                oldPrecision: 6,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Humidity",
                table: "WeatherData",
                type: "double precision",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Fields",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Fields",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Fields");

            migrationBuilder.AlterColumn<decimal>(
                name: "WindSpeed",
                table: "WeatherData",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Temperature",
                table: "WeatherData",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "RainfallMm",
                table: "WeatherData",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldPrecision: 6,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Humidity",
                table: "WeatherData",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);
        }
    }
}
