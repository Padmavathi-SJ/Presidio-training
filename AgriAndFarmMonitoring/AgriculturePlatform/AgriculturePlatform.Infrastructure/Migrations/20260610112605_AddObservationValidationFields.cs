using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriculturePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddObservationValidationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Observations_Admins_AdminId",
                table: "Observations");

            migrationBuilder.DropForeignKey(
                name: "FK_Observations_CropCycles_CropCycleId",
                table: "Observations");

            migrationBuilder.DropForeignKey(
                name: "FK_Observations_Fields_FieldId",
                table: "Observations");

            migrationBuilder.DropForeignKey(
                name: "FK_Observations_Workers_WorkerId",
                table: "Observations");

            migrationBuilder.DropIndex(
                name: "IX_Observations_FarmId",
                table: "Observations");

            migrationBuilder.RenameIndex(
                name: "IX_Observations_FieldId_ObservationDate",
                table: "Observations",
                newName: "IX_Observations_Field_Date");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Observations",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdminId1",
                table: "Observations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "Observations",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "Admin's questions or comments on the observation");

            migrationBuilder.AddColumn<string>(
                name: "FlagReason",
                table: "Observations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "outlier, inconsistent_data, missing_info, duplicate");

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidatedAt",
                table: "Observations",
                type: "timestamp with time zone",
                nullable: true,
                comment: "When the observation was validated");

            migrationBuilder.AddColumn<int>(
                name: "ValidatedBy",
                table: "Observations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidationStatus",
                table: "Observations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "pending",
                comment: "pending, verified, questioned, invalid");

            migrationBuilder.AddColumn<string>(
                name: "WorkerResponse",
                table: "Observations",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "Worker's response to admin questions");

            migrationBuilder.CreateIndex(
                name: "IX_Observations_AdminId1",
                table: "Observations",
                column: "AdminId1");

            migrationBuilder.CreateIndex(
                name: "IX_Observations_Date",
                table: "Observations",
                column: "ObservationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Observations_Farm_ValidationStatus",
                table: "Observations",
                columns: new[] { "FarmId", "ValidationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Observations_ValidatedBy",
                table: "Observations",
                column: "ValidatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Observations_ValidationStatus",
                table: "Observations",
                column: "ValidationStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_Observations_Admins_AdminId",
                table: "Observations",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Observations_Admins_AdminId1",
                table: "Observations",
                column: "AdminId1",
                principalTable: "Admins",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Observations_Admins_ValidatedBy",
                table: "Observations",
                column: "ValidatedBy",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Observations_CropCycles_CropCycleId",
                table: "Observations",
                column: "CropCycleId",
                principalTable: "CropCycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Observations_Fields_FieldId",
                table: "Observations",
                column: "FieldId",
                principalTable: "Fields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Observations_Workers_WorkerId",
                table: "Observations",
                column: "WorkerId",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Observations_Admins_AdminId",
                table: "Observations");

            migrationBuilder.DropForeignKey(
                name: "FK_Observations_Admins_AdminId1",
                table: "Observations");

            migrationBuilder.DropForeignKey(
                name: "FK_Observations_Admins_ValidatedBy",
                table: "Observations");

            migrationBuilder.DropForeignKey(
                name: "FK_Observations_CropCycles_CropCycleId",
                table: "Observations");

            migrationBuilder.DropForeignKey(
                name: "FK_Observations_Fields_FieldId",
                table: "Observations");

            migrationBuilder.DropForeignKey(
                name: "FK_Observations_Workers_WorkerId",
                table: "Observations");

            migrationBuilder.DropIndex(
                name: "IX_Observations_AdminId1",
                table: "Observations");

            migrationBuilder.DropIndex(
                name: "IX_Observations_Date",
                table: "Observations");

            migrationBuilder.DropIndex(
                name: "IX_Observations_Farm_ValidationStatus",
                table: "Observations");

            migrationBuilder.DropIndex(
                name: "IX_Observations_ValidatedBy",
                table: "Observations");

            migrationBuilder.DropIndex(
                name: "IX_Observations_ValidationStatus",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "AdminId1",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "FlagReason",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "ValidatedAt",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "ValidatedBy",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "ValidationStatus",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "WorkerResponse",
                table: "Observations");

            migrationBuilder.RenameIndex(
                name: "IX_Observations_Field_Date",
                table: "Observations",
                newName: "IX_Observations_FieldId_ObservationDate");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Observations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Observations_FarmId",
                table: "Observations",
                column: "FarmId");

            migrationBuilder.AddForeignKey(
                name: "FK_Observations_Admins_AdminId",
                table: "Observations",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Observations_CropCycles_CropCycleId",
                table: "Observations",
                column: "CropCycleId",
                principalTable: "CropCycles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Observations_Fields_FieldId",
                table: "Observations",
                column: "FieldId",
                principalTable: "Fields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Observations_Workers_WorkerId",
                table: "Observations",
                column: "WorkerId",
                principalTable: "Workers",
                principalColumn: "Id");
        }
    }
}
