using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecosystem_backend.Migrations
{
    /// <inheritdoc />
    public partial class AjusteFlujoDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Usuarios_IdUsuario",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_IdUsuario",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "DireccionInstalacion",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "IdUsuario",
                table: "Clientes");

            migrationBuilder.AddColumn<string>(
                name: "Estatus",
                table: "Cotizaciones",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Clientes",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "NombreCompleto",
                table: "Clientes",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Corporativo",
                table: "Clientes",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRegistro",
                table: "Clientes",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "IdProspecto",
                table: "Clientes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Localidad",
                table: "Clientes",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_IdProspecto",
                table: "Clientes",
                column: "IdProspecto");

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Prospectos_IdProspecto",
                table: "Clientes",
                column: "IdProspecto",
                principalTable: "Prospectos",
                principalColumn: "IdProspecto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Prospectos_IdProspecto",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_IdProspecto",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Estatus",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "Corporativo",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "FechaRegistro",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "IdProspecto",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Localidad",
                table: "Clientes");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Clientes",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "NombreCompleto",
                table: "Clientes",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DireccionInstalacion",
                table: "Clientes",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "IdUsuario",
                table: "Clientes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_IdUsuario",
                table: "Clientes",
                column: "IdUsuario",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Usuarios_IdUsuario",
                table: "Clientes",
                column: "IdUsuario",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
