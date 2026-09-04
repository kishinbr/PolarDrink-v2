using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolarDrinks.Migrations
{
    /// <inheritdoc />
    public partial class criandobd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fornecedores",
                columns: table => new
                {
                    FornecedorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FornecedorNome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FornecedorCNPJ = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: false),
                    FornecedorTelefone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    FornecedorEmail = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FornecedorCEP = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FornecedorEstado = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    FornecedorCidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FornecedorBairro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FornecedorLogradouro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FornecedorNum = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FornecedorAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fornecedores", x => x.FornecedorID);
                });

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    ProdutoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProdutoNome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProdutoDescricao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProdutoPrecoCusto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProdutoPrecoVenda = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProdutoPromocao = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ProdutoCodBarra = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProdutoQtdEstoque = table.Column<int>(type: "int", nullable: false),
                    ProdutoEstoqueMinimo = table.Column<int>(type: "int", nullable: false),
                    ProdutoAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.ProdutoID);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioNome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UsuarioLogin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UsuarioSenhaHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuarioPerfil = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuarioAtivo = table.Column<bool>(type: "bit", nullable: false),
                    UsuarioCriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioID);
                });

            migrationBuilder.CreateTable(
                name: "ComprasEstoque",
                columns: table => new
                {
                    CompraID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompraData = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompraDataEntrega = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompraStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FornecedorID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprasEstoque", x => x.CompraID);
                    table.ForeignKey(
                        name: "FK_ComprasEstoque_Fornecedores_FornecedorID",
                        column: x => x.FornecedorID,
                        principalTable: "Fornecedores",
                        principalColumn: "FornecedorID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vendas",
                columns: table => new
                {
                    VendaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendaValorTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VendaTipoPagamento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    VendaData = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VendaCancelada = table.Column<bool>(type: "bit", nullable: false),
                    UsuarioID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendas", x => x.VendaID);
                    table.ForeignKey(
                        name: "FK_Vendas_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID");
                });

            migrationBuilder.CreateTable(
                name: "ItensCompra",
                columns: table => new
                {
                    ItemCompraID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemCompraQtd = table.Column<int>(type: "int", nullable: false),
                    ItemCompraPreco = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProdutoID = table.Column<int>(type: "int", nullable: false),
                    CompraID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensCompra", x => x.ItemCompraID);
                    table.ForeignKey(
                        name: "FK_ItensCompra_ComprasEstoque_CompraID",
                        column: x => x.CompraID,
                        principalTable: "ComprasEstoque",
                        principalColumn: "CompraID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItensCompra_Produtos_ProdutoID",
                        column: x => x.ProdutoID,
                        principalTable: "Produtos",
                        principalColumn: "ProdutoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItensVenda",
                columns: table => new
                {
                    ItemVendaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemVendaQtd = table.Column<int>(type: "int", nullable: false),
                    ItemVendaPreco = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemVendaCusto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemVendaTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProdutoID = table.Column<int>(type: "int", nullable: false),
                    VendaID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensVenda", x => x.ItemVendaID);
                    table.ForeignKey(
                        name: "FK_ItensVenda_Produtos_ProdutoID",
                        column: x => x.ProdutoID,
                        principalTable: "Produtos",
                        principalColumn: "ProdutoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItensVenda_Vendas_VendaID",
                        column: x => x.VendaID,
                        principalTable: "Vendas",
                        principalColumn: "VendaID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimentacoesEstoque",
                columns: table => new
                {
                    MovimentacaoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovimentacaoTipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    MovimentacaoQtd = table.Column<int>(type: "int", nullable: false),
                    MovimentacaoDescricao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MovimentacaoData = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProdutoID = table.Column<int>(type: "int", nullable: false),
                    ItemCompraID = table.Column<int>(type: "int", nullable: true),
                    ItemVendaID = table.Column<int>(type: "int", nullable: true),
                    UsuarioID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesEstoque", x => x.MovimentacaoID);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_ItensCompra_ItemCompraID",
                        column: x => x.ItemCompraID,
                        principalTable: "ItensCompra",
                        principalColumn: "ItemCompraID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_ItensVenda_ItemVendaID",
                        column: x => x.ItemVendaID,
                        principalTable: "ItensVenda",
                        principalColumn: "ItemVendaID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Produtos_ProdutoID",
                        column: x => x.ProdutoID,
                        principalTable: "Produtos",
                        principalColumn: "ProdutoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComprasEstoque_FornecedorID",
                table: "ComprasEstoque",
                column: "FornecedorID");

            migrationBuilder.CreateIndex(
                name: "IX_Fornecedores_FornecedorCNPJ",
                table: "Fornecedores",
                column: "FornecedorCNPJ",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItensCompra_CompraID",
                table: "ItensCompra",
                column: "CompraID");

            migrationBuilder.CreateIndex(
                name: "IX_ItensCompra_ProdutoID",
                table: "ItensCompra",
                column: "ProdutoID");

            migrationBuilder.CreateIndex(
                name: "IX_ItensVenda_ProdutoID",
                table: "ItensVenda",
                column: "ProdutoID");

            migrationBuilder.CreateIndex(
                name: "IX_ItensVenda_VendaID",
                table: "ItensVenda",
                column: "VendaID");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_ItemCompraID",
                table: "MovimentacoesEstoque",
                column: "ItemCompraID");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_ItemVendaID",
                table: "MovimentacoesEstoque",
                column: "ItemVendaID");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_ProdutoID",
                table: "MovimentacoesEstoque",
                column: "ProdutoID");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_UsuarioID",
                table: "MovimentacoesEstoque",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_UsuarioLogin",
                table: "Usuarios",
                column: "UsuarioLogin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_UsuarioID",
                table: "Vendas",
                column: "UsuarioID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimentacoesEstoque");

            migrationBuilder.DropTable(
                name: "ItensCompra");

            migrationBuilder.DropTable(
                name: "ItensVenda");

            migrationBuilder.DropTable(
                name: "ComprasEstoque");

            migrationBuilder.DropTable(
                name: "Produtos");

            migrationBuilder.DropTable(
                name: "Vendas");

            migrationBuilder.DropTable(
                name: "Fornecedores");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
