using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NotificationSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    phone_num = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    receiveemailnotification = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    receivesmsnotification = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    user_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    subject = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    message = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    recipient = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_sent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_notification_is_sent",
                table: "notifications",
                column: "is_sent");

            migrationBuilder.CreateIndex(
                name: "idx_notification_sent_at",
                table: "notifications",
                column: "sent_at");

            migrationBuilder.CreateIndex(
                name: "idx_notification_type",
                table: "notifications",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "idx_notification_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
