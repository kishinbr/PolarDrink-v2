using Microsoft.EntityFrameworkCore;
using PolarDrinks.Models;
using PolarDrinks.Models.Loja;

namespace PolarDrinks.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ProdutoModel> Produtos { get; set; }
        public DbSet<FornecedorModel> Fornecedores { get; set; }
        public DbSet<CompraEstoqueModel> ComprasEstoque { get; set; }
        public DbSet<ItemCompraModel> ItensCompra { get; set; }
        public DbSet<VendaModel> Vendas { get; set; }
        public DbSet<ItemVendaModel> ItensVenda { get; set; }
        public DbSet<MovimentacaoEstoqueModel> MovimentacoesEstoque { get; set; }
        public DbSet<UsuarioModel> Usuarios { get; set; }

        // ===== LOJA ONLINE =====
        public DbSet<ClienteModel> Clientes { get; set; }
        public DbSet<CategoriaModel> Categorias { get; set; }
        public DbSet<ProdutoCategoriaModel> ProdutoCategorias { get; set; }
        public DbSet<CarrinhoItemModel> CarrinhoItens { get; set; }
        public DbSet<PedidoModel> Pedidos { get; set; }
        public DbSet<ItemPedidoModel> ItensPedido { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ================= RELACIONAMENTOS =================

            // COMPRA -> FORNECEDOR
            modelBuilder.Entity<CompraEstoqueModel>()
                .HasOne(c => c.Fornecedor)
                .WithMany(f => f.Compras)
                .HasForeignKey(c => c.FornecedorID);

            modelBuilder.Entity<FornecedorModel>()
                .HasIndex(f => f.FornecedorCNPJ)
                .IsUnique();
            modelBuilder.Entity<ProdutoModel>()
                .HasIndex(p => p.ProdutoCodBarra)
                .IsUnique();    

            modelBuilder.Entity<CompraEstoqueModel>()
                .Ignore(c => c.CompraValorTotal);

            // ITEM COMPRA -> COMPRA
            modelBuilder.Entity<ItemCompraModel>()
                .HasOne(ic => ic.Compra)
                .WithMany(c => c.Itens)
                .HasForeignKey(ic => ic.CompraID);

            // ITEM COMPRA -> PRODUTO
            modelBuilder.Entity<ItemCompraModel>()
                .HasOne(ic => ic.Produto)
                .WithMany()
                .HasForeignKey(ic => ic.ProdutoID);

            // ITEM VENDA -> VENDA
            modelBuilder.Entity<ItemVendaModel>()
                .HasOne(iv => iv.Venda)
                .WithMany(v => v.Itens)
                .HasForeignKey(iv => iv.VendaID);

            // ITEM VENDA -> PRODUTO
            modelBuilder.Entity<ItemVendaModel>()
                .HasOne(iv => iv.Produto)
                .WithMany()
                .HasForeignKey(iv => iv.ProdutoID);

            // MOVIMENTAÇÃO -> PRODUTO
            modelBuilder.Entity<MovimentacaoEstoqueModel>()
                .HasOne(m => m.Produto)
                .WithMany()
                .HasForeignKey(m => m.ProdutoID);

            // MOVIMENTAÇÃO -> ITEM COMPRA
            modelBuilder.Entity<MovimentacaoEstoqueModel>()
                .HasOne(m => m.ItemCompra)
                .WithMany()
                .HasForeignKey(m => m.ItemCompraID)
                .OnDelete(DeleteBehavior.Restrict);

            // MOVIMENTAÇÃO -> ITEM VENDA
            modelBuilder.Entity<MovimentacaoEstoqueModel>()
                .HasOne(m => m.ItemVenda)
                .WithMany()
                .HasForeignKey(m => m.ItemVendaID)
                .OnDelete(DeleteBehavior.Restrict);

            // PRODUTO
            modelBuilder.Entity<ProdutoModel>()
                .Property(p => p.ProdutoPrecoVenda)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProdutoModel>()
                .Property(p => p.ProdutoPrecoCusto)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProdutoModel>()
                .Property(p => p.ProdutoPromocao)
                .HasPrecision(5, 2);

            // ITEM COMPRA
            modelBuilder.Entity<ItemCompraModel>()
                .Property(ic => ic.ItemCompraPreco)
                .HasPrecision(18, 2);

            // ITEM VENDA
            modelBuilder.Entity<ItemVendaModel>()
                .Property(iv => iv.ItemVendaPreco)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ItemVendaModel>()
                .Property(iv => iv.ItemVendaCusto)
                .HasPrecision(18, 2);

            // VENDA FINAL
            modelBuilder.Entity<VendaModel>()
                .Property(v => v.VendaValorTotal)
                .HasPrecision(18, 2);

            // USUARIO — login único
            modelBuilder.Entity<UsuarioModel>()
                .HasIndex(u => u.UsuarioLogin)
                .IsUnique();

            modelBuilder.Entity<VendaModel>()
                .HasOne(v => v.Usuario)
                .WithMany()
                .HasForeignKey(v => v.UsuarioID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimentacaoEstoqueModel>()
                .HasOne(m => m.Usuario)
                .WithMany()
                .HasForeignKey(m => m.UsuarioID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CompraEstoqueModel>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioID)
                .OnDelete(DeleteBehavior.Restrict);

            // ================= LOJA ONLINE =================

            // CLIENTE — e-mail e CPF únicos
            modelBuilder.Entity<ClienteModel>()
                .HasIndex(c => c.ClienteEmail)
                .IsUnique();

            modelBuilder.Entity<ClienteModel>()
                .HasIndex(c => c.ClienteCPF)
                .IsUnique();

            // PRODUTO_CATEGORIA — chave composta (Produto + Categoria)
            modelBuilder.Entity<ProdutoCategoriaModel>()
                .HasKey(pc => new { pc.ProdutoID, pc.CategoriaID });

            modelBuilder.Entity<ProdutoCategoriaModel>()
                .HasOne(pc => pc.Produto)
                .WithMany()
                .HasForeignKey(pc => pc.ProdutoID);

            modelBuilder.Entity<ProdutoCategoriaModel>()
                .HasOne(pc => pc.Categoria)
                .WithMany()
                .HasForeignKey(pc => pc.CategoriaID);

            // CARRINHO ITEM -> CLIENTE
            modelBuilder.Entity<CarrinhoItemModel>()
                .HasOne(ci => ci.Cliente)
                .WithMany()
                .HasForeignKey(ci => ci.ClienteID)
                .OnDelete(DeleteBehavior.Cascade);

            // CARRINHO ITEM -> PRODUTO
            modelBuilder.Entity<CarrinhoItemModel>()
                .HasOne(ci => ci.Produto)
                .WithMany()
                .HasForeignKey(ci => ci.ProdutoID)
                .OnDelete(DeleteBehavior.Restrict);

            // PEDIDO -> CLIENTE
            modelBuilder.Entity<PedidoModel>()
                .HasOne(p => p.Cliente)
                .WithMany()
                .HasForeignKey(p => p.ClienteID)
                .OnDelete(DeleteBehavior.Restrict);

            // PEDIDO -> USUARIO (separou / entregou)
            modelBuilder.Entity<PedidoModel>()
                .HasOne(p => p.UsuarioSeparou)
                .WithMany()
                .HasForeignKey(p => p.UsuarioSeparouID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PedidoModel>()
                .HasOne(p => p.UsuarioEntregou)
                .WithMany()
                .HasForeignKey(p => p.UsuarioEntregouID)
                .OnDelete(DeleteBehavior.Restrict);

            // ITEM PEDIDO -> PEDIDO
            modelBuilder.Entity<ItemPedidoModel>()
                .HasOne(ip => ip.Pedido)
                .WithMany(p => p.Itens)
                .HasForeignKey(ip => ip.PedidoID);

            // ITEM PEDIDO -> PRODUTO
            modelBuilder.Entity<ItemPedidoModel>()
                .HasOne(ip => ip.Produto)
                .WithMany()
                .HasForeignKey(ip => ip.ProdutoID)
                .OnDelete(DeleteBehavior.Restrict);

            // MOVIMENTAÇÃO -> ITEM PEDIDO
            modelBuilder.Entity<MovimentacaoEstoqueModel>()
                .HasOne(m => m.ItemPedido)
                .WithMany()
                .HasForeignKey(m => m.ItemPedidoID)
                .OnDelete(DeleteBehavior.Restrict);

            // Precisões decimais (padrão do projeto)
            modelBuilder.Entity<ItemPedidoModel>()
                .Property(ip => ip.ItemPedidoPreco)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ItemPedidoModel>()
                .Property(ip => ip.ItemPedidoCusto)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PedidoModel>()
                .Property(p => p.PedidoValorTotal)
                .HasPrecision(18, 2);

        }
    }
}