using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriculturePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkerIdToRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AdminId",
                table: "RefreshTokens",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "WorkerId",
                table: "RefreshTokens",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_WorkerId_IsRevoked",
                table: "RefreshTokens",
                columns: new[] { "WorkerId", "IsRevoked" });

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Workers_WorkerId",
                table: "RefreshTokens",
                column: "WorkerId",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Workers_WorkerId",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_WorkerId_IsRevoked",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "WorkerId",
                table: "RefreshTokens");

            migrationBuilder.AlterColumn<int>(
                name: "AdminId",
                table: "RefreshTokens",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
