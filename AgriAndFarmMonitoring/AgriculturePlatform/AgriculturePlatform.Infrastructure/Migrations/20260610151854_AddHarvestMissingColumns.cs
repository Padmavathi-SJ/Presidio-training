using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriculturePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHarvestMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Harvests_Admins_AdminId",
                table: "Harvests");

            migrationBuilder.DropForeignKey(
                name: "FK_Harvests_Admins_ApprovedBy",
                table: "Harvests");

            migrationBuilder.DropForeignKey(
                name: "FK_Harvests_CropCycles_CropCycleId",
                table: "Harvests");

            migrationBuilder.DropForeignKey(
                name: "FK_Harvests_Fields_FieldId",
                table: "Harvests");

            migrationBuilder.DropForeignKey(
                name: "FK_Harvests_Workers_HarvestedBy",
                table: "Harvests");

            migrationBuilder.RenameIndex(
                name: "IX_Harvests_FarmId_HarvestDate",
                table: "Harvests",
                newName: "IX_Harvests_Farm_Date");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Harvests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApprovalStatus",
                table: "Harvests",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "PENDING",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdminId1",
                table: "Harvests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "Harvests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                table: "Harvests",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FieldId1",
                table: "Harvests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HarvestMethod",
                table: "Harvests",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerKg",
                table: "Harvests",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Harvests",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubmittedBy",
                table: "Harvests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkerResponse",
                table: "Harvests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Harvests_AdminId1",
                table: "Harvests",
                column: "AdminId1");

            migrationBuilder.CreateIndex(
                name: "IX_Harvests_BatchNumber",
                table: "Harvests",
                column: "BatchNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Harvests_FieldId1",
                table: "Harvests",
                column: "FieldId1");

            migrationBuilder.CreateIndex(
                name: "IX_Harvests_SubmittedBy",
                table: "Harvests",
                column: "SubmittedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Harvests_Admins_AdminId",
                table: "Harvests",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Harvests_Admins_AdminId1",
                table: "Harvests",
                column: "AdminId1",
                principalTable: "Admins",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Harvests_Admins_ApprovedBy",
                table: "Harvests",
                column: "ApprovedBy",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Harvests_CropCycles_CropCycleId",
                table: "Harvests",
                column: "CropCycleId",
                principalTable: "CropCycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Harvests_Fields_FieldId",
                table: "Harvests",
                column: "FieldId",
                principalTable: "Fields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Harvests_Fields_FieldId1",
                table: "Harvests",
                column: "FieldId1",
                principalTable: "Fields",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Harvests_Workers_HarvestedBy",
                table: "Harvests",
                column: "HarvestedBy",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Harvests_Workers_SubmittedBy",
                table: "Harvests",
                column: "SubmittedBy",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Harvests_Admins_AdminId",
                table: "Harvests");

            migrationBuilder.DropForeignKey(
                name: "FK_Harvests_Admins_AdminId1",
                table: "Harvests");

            migrationBuilder.DropForeignKey(
                name: "FK_Harvests_Admins_ApprovedBy",
                table: "Harvests");

            migrationBuilder.DropForeignKey(
                name: "FK_Harvests_CropCycles_CropCycleId",
                table: "Harvests");

            migrationBuilder.DropForeignKey(
                name: "FK_Harvests_Fields_FieldId",
                table: "Harvests");

            migrationBuilder.DropForeignKey(
                name: "FK_Harvests_Fields_FieldId1",
                table: "Harvests");

            migrationBuilder.DropForeignKey(
                name: "FK_Harvests_Workers_HarvestedBy",
                table: "Harvests");

            migrationBuilder.DropForeignKey(
                name: "FK_Harvests_Workers_SubmittedBy",
                table: "Harvests");

            migrationBuilder.DropIndex(
                name: "IX_Harvests_AdminId1",
                table: "Harvests");

            migrationBuilder.DropIndex(
                name: "IX_Harvests_BatchNumber",
                table: "Harvests");

            migrationBuilder.DropIndex(
                name: "IX_Harvests_FieldId1",
                table: "Harvests");

            migrationBuilder.DropIndex(
                name: "IX_Harvests_SubmittedBy",
                table: "Harvests");

            migrationBuilder.DropColumn(
                name: "AdminId1",
                table: "Harvests");

            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "Harvests");

            migrationBuilder.DropColumn(
                name: "BatchNumber",
                table: "Harvests");

            migrationBuilder.DropColumn(
                name: "FieldId1",
                table: "Harvests");

            migrationBuilder.DropColumn(
                name: "HarvestMethod",
                table: "Harvests");

            migrationBuilder.DropColumn(
                name: "PricePerKg",
                table: "Harvests");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Harvests");

            migrationBuilder.DropColumn(
                name: "SubmittedBy",
                table: "Harvests");

            migrationBuilder.DropColumn(
                name: "WorkerResponse",
                table: "Harvests");

            migrationBuilder.RenameIndex(
                name: "IX_Harvests_Farm_Date",
                table: "Harvests",
                newName: "IX_Harvests_FarmId_HarvestDate");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Harvests",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApprovalStatus",
                table: "Harvests",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "PENDING");

            migrationBuilder.AddForeignKey(
                name: "FK_Harvests_Admins_AdminId",
                table: "Harvests",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Harvests_Admins_ApprovedBy",
                table: "Harvests",
                column: "ApprovedBy",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Harvests_CropCycles_CropCycleId",
                table: "Harvests",
                column: "CropCycleId",
                principalTable: "CropCycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Harvests_Fields_FieldId",
                table: "Harvests",
                column: "FieldId",
                principalTable: "Fields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Harvests_Workers_HarvestedBy",
                table: "Harvests",
                column: "HarvestedBy",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
