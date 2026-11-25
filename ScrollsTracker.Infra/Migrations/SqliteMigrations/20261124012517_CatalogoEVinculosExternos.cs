using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScrollsTracker.Infra.Migrations.SqliteMigrations
{
    /// <inheritdoc />
    public partial class CatalogoEVinculosExternos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TitulosAlternativos",
                table: "Obras",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "'[]'");

            migrationBuilder.AddColumn<string>(
                name: "VinculosExternos",
                table: "Obras",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "'{}'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TitulosAlternativos",
                table: "Obras");

            migrationBuilder.DropColumn(
                name: "VinculosExternos",
                table: "Obras");
        }
    }
}
