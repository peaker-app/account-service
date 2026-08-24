using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileAscents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "public_distinct_peaks",
                table: "profile_stats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "public_highest_altitude_m",
                table: "profile_stats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "public_highest_peak_id",
                table: "profile_stats",
                type: "char(36)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "public_highest_peak_name",
                table: "profile_stats",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "public_last_ascent_date",
                table: "profile_stats",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "public_total_ascents",
                table: "profile_stats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "profile_ascents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    profile_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ascent_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    peak_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    peak_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    peak_altitude_m = table.Column<int>(type: "int", nullable: false),
                    ascent_date = table.Column<DateTime>(type: "date", nullable: false),
                    visibility = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profile_ascents", x => x.id);
                    table.ForeignKey(
                        name: "FK_profile_ascents_profiles_profile_id",
                        column: x => x.profile_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_profile_ascents_peak",
                table: "profile_ascents",
                column: "peak_id");

            migrationBuilder.CreateIndex(
                name: "ix_profile_ascents_profile",
                table: "profile_ascents",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "ux_profile_ascents_ascent",
                table: "profile_ascents",
                column: "ascent_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "profile_ascents");

            migrationBuilder.DropColumn(
                name: "public_distinct_peaks",
                table: "profile_stats");

            migrationBuilder.DropColumn(
                name: "public_highest_altitude_m",
                table: "profile_stats");

            migrationBuilder.DropColumn(
                name: "public_highest_peak_id",
                table: "profile_stats");

            migrationBuilder.DropColumn(
                name: "public_highest_peak_name",
                table: "profile_stats");

            migrationBuilder.DropColumn(
                name: "public_last_ascent_date",
                table: "profile_stats");

            migrationBuilder.DropColumn(
                name: "public_total_ascents",
                table: "profile_stats");
        }
    }
}
