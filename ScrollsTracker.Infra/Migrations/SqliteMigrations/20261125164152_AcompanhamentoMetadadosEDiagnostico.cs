using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScrollsTracker.Infra.Migrations.SqliteMigrations
{
    /// <inheritdoc />
    public partial class AcompanhamentoMetadadosEDiagnostico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LeituraAtualizadaEm",
                table: "UsuarioObras",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Seguindo",
                table: "UsuarioObras",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsultaFontesEm",
                table: "Obras",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FixarDescricao",
                table: "Obras",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FixarImagem",
                table: "Obras",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FixarStatus",
                table: "Obras",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "FonteDescricao",
                table: "Obras",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FonteImagem",
                table: "Obras",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FonteStatus",
                table: "Obras",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UltimaConsultaFontes",
                table: "Obras",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "'{}'");

            migrationBuilder.CreateTable(
                name: "PreferenciasResumo",
                columns: table => new
                {
                    UsuarioId = table.Column<string>(type: "TEXT", nullable: false),
                    Frequencia = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreferenciasResumo", x => x.UsuarioId);
                    table.ForeignKey(
                        name: "FK_PreferenciasResumo_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreferenciasResumo");

            migrationBuilder.DropColumn(
                name: "LeituraAtualizadaEm",
                table: "UsuarioObras");

            migrationBuilder.DropColumn(
                name: "Seguindo",
                table: "UsuarioObras");

            migrationBuilder.DropColumn(
                name: "ConsultaFontesEm",
                table: "Obras");

            migrationBuilder.DropColumn(
                name: "FixarDescricao",
                table: "Obras");

            migrationBuilder.DropColumn(
                name: "FixarImagem",
                table: "Obras");

            migrationBuilder.DropColumn(
                name: "FixarStatus",
                table: "Obras");

            migrationBuilder.DropColumn(
                name: "FonteDescricao",
                table: "Obras");

            migrationBuilder.DropColumn(
                name: "FonteImagem",
                table: "Obras");

            migrationBuilder.DropColumn(
                name: "FonteStatus",
                table: "Obras");

            migrationBuilder.DropColumn(
                name: "UltimaConsultaFontes",
                table: "Obras");
        }
    }
}
