using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Persistence.Data.Migrations.CoreMigrations
{
    /// <inheritdoc />
    public partial class AddWorldNameTrigramIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_World_Name",
                schema: "public",
                table: "World",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_World_Name",
                schema: "public",
                table: "World");
        }
    }
}
