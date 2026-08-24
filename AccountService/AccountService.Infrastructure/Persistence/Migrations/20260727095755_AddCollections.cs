using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "collections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    profile_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    kind = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false, collation: "utf8mb4_0900_ai_ci"),
                    description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collections", x => x.id);
                    table.ForeignKey(
                        name: "FK_collections_profiles_profile_id",
                        column: x => x.profile_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "collection_peaks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    peak_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    peak_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    peak_altitude_m = table.Column<int>(type: "int", nullable: false),
                    added_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    collection_id = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collection_peaks", x => x.id);
                    table.ForeignKey(
                        name: "FK_collection_peaks_collections_collection_id",
                        column: x => x.collection_id,
                        principalTable: "collections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            // Motivo: MySql.EntityFrameworkCore ignora UseCollation al emitir el DDL; sin este MODIFY la
            // unicidad del nombre dependería de la colación por defecto del servidor (DESIGN.md §5.1).
            migrationBuilder.Sql(
                "ALTER TABLE `collections` MODIFY `name` VARCHAR(60) "
                + "CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL;");

            migrationBuilder.CreateIndex(
                name: "ix_collection_peaks_peak",
                table: "collection_peaks",
                column: "peak_id");

            migrationBuilder.CreateIndex(
                name: "ux_collection_peaks_unique",
                table: "collection_peaks",
                columns: new[] { "collection_id", "peak_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_collections_profile_name",
                table: "collections",
                columns: new[] { "profile_id", "name" },
                unique: true);

            // Motivo: MySql.EntityFrameworkCore no genera DDL de columnas generadas y MySQL 8.4 no
            // admite índices parciales; default_slot es el idioma equivalente (DESIGN.md §5.1).
            migrationBuilder.Sql(
                "ALTER TABLE `collections` ADD COLUMN `default_slot` TINYINT "
                + "GENERATED ALWAYS AS (CASE WHEN `kind` = 'WantToClimb' THEN 1 END) STORED;");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX `ux_collections_profile_default` "
                + "ON `collections` (`profile_id`, `default_slot`);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "collection_peaks");

            migrationBuilder.DropTable(
                name: "collections");
        }
    }
}
