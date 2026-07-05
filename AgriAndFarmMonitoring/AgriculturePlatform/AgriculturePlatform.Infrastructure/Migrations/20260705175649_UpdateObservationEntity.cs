using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriculturePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateObservationEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PestDetected",
                table: "Observations");

            migrationBuilder.AddColumn<List<string>>(
                name: "AdditionalImagePaths",
                table: "Observations",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageCaption",
                table: "Observations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageMetadata",
                table: "Observations",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Observations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageVerificationNotes",
                table: "Observations",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsImageVerified",
                table: "Observations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailPath",
                table: "Observations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalImagePaths",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "ImageCaption",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "ImageMetadata",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "ImageVerificationNotes",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "IsImageVerified",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "ThumbnailPath",
                table: "Observations");

            migrationBuilder.AddColumn<bool>(
                name: "PestDetected",
                table: "Observations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
