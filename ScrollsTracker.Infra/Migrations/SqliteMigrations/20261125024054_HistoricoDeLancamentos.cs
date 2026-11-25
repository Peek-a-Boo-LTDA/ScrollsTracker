using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScrollsTracker.Infra.Migrations.SqliteMigrations
{
    /// <inheritdoc />
    public partial class HistoricoDeLancamentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FonteUltimoCapitulo",
                table: "Obras",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoCapituloDetectadoEm",
                table: "Obras",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Lancamentos",
                columns: table => new
                {
                    ObraId = table.Column<int>(type: "INTEGER", nullable: false),
                    Numero = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Fonte = table.Column<int>(type: "INTEGER", nullable: false),
                    DetectadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lancamentos", x => new { x.ObraId, x.Numero });
                    table.ForeignKey(
                        name: "FK_Lancamentos_Obras_ObraId",
                        column: x => x.ObraId,
                        principalTable: "Obras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Obras_DataVerificacao",
                table: "Obras",
                column: "DataVerificacao");

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_DetectadoEm",
                table: "Lancamentos",
                column: "DetectadoEm");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lancamentos");

            migrationBuilder.DropIndex(
                name: "IX_Obras_DataVerificacao",
                table: "Obras");

            migrationBuilder.DropColumn(
                name: "FonteUltimoCapitulo",
                table: "Obras");

            migrationBuilder.DropColumn(
                name: "UltimoCapituloDetectadoEm",
                table: "Obras");
        }
    }
}
