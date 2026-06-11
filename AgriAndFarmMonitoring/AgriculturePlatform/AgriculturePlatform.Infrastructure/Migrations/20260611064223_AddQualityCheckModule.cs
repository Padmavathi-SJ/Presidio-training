using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriculturePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQualityCheckModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QualityChecks_Admins_ApprovedBy",
                table: "QualityChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityChecks_Workers_CheckedBy",
                table: "QualityChecks");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Workers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Workers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Workers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "Workers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "Workers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "Workers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkerId1",
                table: "WorkerFieldAssignments",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApprovalStatus",
                table: "QualityChecks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "PENDING",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "QualityChecks",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "QualityChecks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "QualityChecks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkerResponse",
                table: "QualityChecks",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkerFieldAssignments_WorkerId1",
                table: "WorkerFieldAssignments",
                column: "WorkerId1");

            migrationBuilder.CreateIndex(
                name: "IX_QualityChecks_CheckDate",
                table: "QualityChecks",
                column: "CheckDate");

            migrationBuilder.AddForeignKey(
                name: "FK_QualityChecks_Admins_ApprovedBy",
                table: "QualityChecks",
                column: "ApprovedBy",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_QualityChecks_Workers_CheckedBy",
                table: "QualityChecks",
                column: "CheckedBy",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerFieldAssignments_Workers_WorkerId1",
                table: "WorkerFieldAssignments",
                column: "WorkerId1",
                principalTable: "Workers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QualityChecks_Admins_ApprovedBy",
                table: "QualityChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityChecks_Workers_CheckedBy",
                table: "QualityChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkerFieldAssignments_Workers_WorkerId1",
                table: "WorkerFieldAssignments");

            migrationBuilder.DropIndex(
                name: "IX_WorkerFieldAssignments_WorkerId1",
                table: "WorkerFieldAssignments");

            migrationBuilder.DropIndex(
                name: "IX_QualityChecks_CheckDate",
                table: "QualityChecks");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "WorkerId1",
                table: "WorkerFieldAssignments");

            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "QualityChecks");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "QualityChecks");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "QualityChecks");

            migrationBuilder.DropColumn(
                name: "WorkerResponse",
                table: "QualityChecks");

            migrationBuilder.AlterColumn<string>(
                name: "ApprovalStatus",
                table: "QualityChecks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "PENDING");

            migrationBuilder.AddForeignKey(
                name: "FK_QualityChecks_Admins_ApprovedBy",
                table: "QualityChecks",
                column: "ApprovedBy",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QualityChecks_Workers_CheckedBy",
                table: "QualityChecks",
                column: "CheckedBy",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
