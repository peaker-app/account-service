using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAccountSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    type = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    content = table.Column<string>(type: "longtext", nullable: false),
                    occurred_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    processed_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    error = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "processed_messages",
                columns: table => new
                {
                    message_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    processed_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_messages", x => x.message_id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    display_name = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    slug = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    bio = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    avatar_public_id = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    avatar_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    country_code = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true),
                    visibility = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profiles", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "profile_stats",
                columns: table => new
                {
                    profile_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    total_ascents = table.Column<int>(type: "int", nullable: false),
                    distinct_peaks = table.Column<int>(type: "int", nullable: false),
                    highest_altitude_m = table.Column<int>(type: "int", nullable: false),
                    highest_peak_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    highest_peak_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    last_ascent_date = table.Column<DateOnly>(type: "date", nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profile_stats", x => x.profile_id);
                    table.ForeignKey(
                        name: "FK_profile_stats_profiles_profile_id",
                        column: x => x.profile_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_at_utc",
                table: "outbox_messages",
                column: "processed_at_utc");

            migrationBuilder.CreateIndex(
                name: "ux_profiles_slug",
                table: "profiles",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_profiles_user_id",
                table: "profiles",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "processed_messages");

            migrationBuilder.DropTable(
                name: "profile_stats");

            migrationBuilder.DropTable(
                name: "profiles");
        }
    }
}
