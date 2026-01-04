using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CoShare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncAuthSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "auth_otp_request",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    purpose = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    otp_code_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    notification_template_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notification_message_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    company_id = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auth_otp_request", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "auth_refresh_token",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    admin_user_id = table.Column<long>(type: "bigint", nullable: true),
                    token_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    device_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    platform = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revoked_reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auth_refresh_token", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_auth_otp_request__expires_at",
                table: "auth_otp_request",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_auth_otp_request__phone_purpose_status",
                table: "auth_otp_request",
                columns: new[] { "phone", "purpose", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_auth_refresh_token__admin_user_id",
                table: "auth_refresh_token",
                column: "admin_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_auth_refresh_token__expires_at",
                table: "auth_refresh_token",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_auth_refresh_token__user_id",
                table: "auth_refresh_token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ux_auth_refresh_token__token_hash",
                table: "auth_refresh_token",
                column: "token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auth_otp_request");

            migrationBuilder.DropTable(
                name: "auth_refresh_token");
        }
    }
}
