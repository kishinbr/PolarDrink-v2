using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolarDrinks.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarLojaOnline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProdutoImagemUrl",
                table: "Produtos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Produtos",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemPedidoID",
                table: "MovimentacoesEstoque",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    CategoriaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoriaNome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CategoriaAtiva = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.CategoriaID);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    ClienteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteNome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClienteEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ClienteSenhaHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClienteTelefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClienteCPF = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    ClienteCriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClienteAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.ClienteID);
                });

            migrationBuilder.CreateTable(
                name: "ProdutoCategorias",
                columns: table => new
                {
                    ProdutoID = table.Column<int>(type: "int", nullable: false),
                    CategoriaID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutoCategorias", x => new { x.ProdutoID, x.CategoriaID });
                    table.ForeignKey(
                        name: "FK_ProdutoCategorias_Categorias_CategoriaID",
                        column: x => x.CategoriaID,
                        principalTable: "Categorias",
                        principalColumn: "CategoriaID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProdutoCategorias_Produtos_ProdutoID",
                        column: x => x.ProdutoID,
                        principalTable: "Produtos",
                        principalColumn: "ProdutoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarrinhoItens",
                columns: table => new
                {
                    CarrinhoItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteID = table.Column<int>(type: "int", nullable: false),
                    ProdutoID = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    AdicionadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarrinhoItens", x => x.CarrinhoItemID);
                    table.ForeignKey(
                        name: "FK_CarrinhoItens_Clientes_ClienteID",
                        column: x => x.ClienteID,
                        principalTable: "Clientes",
                        principalColumn: "ClienteID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarrinhoItens_Produtos_ProdutoID",
                        column: x => x.ProdutoID,
                        principalTable: "Produtos",
                        principalColumn: "ProdutoID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    PedidoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoCodigo = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    ClienteID = table.Column<int>(type: "int", nullable: false),
                    PedidoData = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PedidoStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PedidoValorTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PedidoDataSeparado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioSeparouID = table.Column<int>(type: "int", nullable: true),
                    PedidoDataConcluido = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEntregouID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.PedidoID);
                    table.ForeignKey(
                        name: "FK_Pedidos_Clientes_ClienteID",
                        column: x => x.ClienteID,
                        principalTable: "Clientes",
                        principalColumn: "ClienteID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pedidos_Usuarios_UsuarioEntregouID",
                        column: x => x.UsuarioEntregouID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pedidos_Usuarios_UsuarioSeparouID",
                        column: x => x.UsuarioSeparouID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItensPedido",
                columns: table => new
                {
                    ItemPedidoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoID = table.Column<int>(type: "int", nullable: false),
                    ProdutoID = table.Column<int>(type: "int", nullable: false),
                    ItemPedidoQtd = table.Column<int>(type: "int", nullable: false),
                    ItemPedidoPreco = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemPedidoCusto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensPedido", x => x.ItemPedidoID);
                    table.ForeignKey(
                        name: "FK_ItensPedido_Pedidos_PedidoID",
                        column: x => x.PedidoID,
                        principalTable: "Pedidos",
                        principalColumn: "PedidoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItensPedido_Produtos_ProdutoID",
                        column: x => x.ProdutoID,
                        principalTable: "Produtos",
                        principalColumn: "ProdutoID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_ItemPedidoID",
                table: "MovimentacoesEstoque",
                column: "ItemPedidoID");

            migrationBuilder.CreateIndex(
                name: "IX_CarrinhoItens_ClienteID",
                table: "CarrinhoItens",
                column: "ClienteID");

            migrationBuilder.CreateIndex(
                name: "IX_CarrinhoItens_ProdutoID",
                table: "CarrinhoItens",
                column: "ProdutoID");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_ClienteCPF",
                table: "Clientes",
                column: "ClienteCPF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_ClienteEmail",
                table: "Clientes",
                column: "ClienteEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItensPedido_PedidoID",
                table: "ItensPedido",
                column: "PedidoID");

            migrationBuilder.CreateIndex(
                name: "IX_ItensPedido_ProdutoID",
                table: "ItensPedido",
                column: "ProdutoID");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_ClienteID",
                table: "Pedidos",
                column: "ClienteID");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_UsuarioEntregouID",
                table: "Pedidos",
                column: "UsuarioEntregouID");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_UsuarioSeparouID",
                table: "Pedidos",
                column: "UsuarioSeparouID");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoCategorias_CategoriaID",
                table: "ProdutoCategorias",
                column: "CategoriaID");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacoesEstoque_ItensPedido_ItemPedidoID",
                table: "MovimentacoesEstoque",
                column: "ItemPedidoID",
                principalTable: "ItensPedido",
                principalColumn: "ItemPedidoID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacoesEstoque_ItensPedido_ItemPedidoID",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropTable(
                name: "CarrinhoItens");

            migrationBuilder.DropTable(
                name: "ItensPedido");

            migrationBuilder.DropTable(
                name: "ProdutoCategorias");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_MovimentacoesEstoque_ItemPedidoID",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropColumn(
                name: "ProdutoImagemUrl",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "ItemPedidoID",
                table: "MovimentacoesEstoque");
        }
    }
}
