using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolarDrinks.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTipoPagamentoPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PedidoTipoPagamento",
                table: "Pedidos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PedidoTipoPagamento",
                table: "Pedidos");
        }
    }
}
