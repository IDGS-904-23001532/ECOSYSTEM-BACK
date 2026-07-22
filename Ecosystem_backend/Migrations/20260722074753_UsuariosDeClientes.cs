using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecosystem_backend.Migrations
{
    /// <inheritdoc />
    public partial class UsuariosDeClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdUsuario",
                table: "Clientes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_IdUsuario",
                table: "Clientes",
                column: "IdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Usuarios_IdUsuario",
                table: "Clientes",
                column: "IdUsuario",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Usuarios_IdUsuario",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_IdUsuario",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "IdUsuario",
                table: "Clientes");
        }
    }
}
