using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriculturePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "AdditionalImagePaths",
                table: "Fields",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageCaption",
                table: "Fields",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Fields",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailPath",
                table: "Fields",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalImagePaths",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "ImageCaption",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "ThumbnailPath",
                table: "Fields");
        }
    }
}
