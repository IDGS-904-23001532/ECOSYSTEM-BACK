using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecosystem_backend.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPrecioUnitarioADetalleCotizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecioUnitario",
                table: "DetallesCotizaciones",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecioUnitario",
                table: "DetallesCotizaciones");
        }
    }
}
