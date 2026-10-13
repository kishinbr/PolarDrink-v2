using PolarDrinks.Models;

namespace PolarDrinks.Tests.Integracao;

[Collection(ColecaoBancoDeTestes.Nome)]
[Trait("Categoria", "Integracao")]
public class BancoDeTestesFumacaTests
{
    private readonly BancoDeTestes _banco;

    public BancoDeTestesFumacaTests(BancoDeTestes banco)
    {
        _banco = banco;
        _banco.LimparDados();
    }

    [FatoIntegracao]
    public void BancoDeTestes_DeveGravarELerUmProduto()
    {
        using (var db = _banco.CriarContexto())
        {
            db.Produtos.Add(new ProdutoModel
            {
                ProdutoNome = "Produto de teste",
                ProdutoCodBarra = "7890000000001",
                ProdutoPrecoVenda = 10m,
                ProdutoPrecoCusto = 5m,
                ProdutoQtdEstoque = 3,
                ProdutoEstoqueMinimo = 1,
                ProdutoAtivo = true
            });

            db.SaveChanges();
        }

        // Contexto novo: lê o que realmente ficou gravado no banco
        using (var db = _banco.CriarContexto())
        {
            var produto = db.Produtos.Single();

            Assert.Equal("Produto de teste", produto.ProdutoNome);
            Assert.Equal(3, produto.ProdutoQtdEstoque);
            Assert.NotEmpty(produto.RowVersion); // gerado pelo próprio SQL Server
        }
    }

    [FatoIntegracao]
    public void LimparDados_DeveDeixarOBancoVazio()
    {
        using (var db = _banco.CriarContexto())
        {
            db.Produtos.Add(new ProdutoModel
            {
                ProdutoNome = "Outro produto",
                ProdutoCodBarra = "7890000000002",
                ProdutoPrecoVenda = 10m,
                ProdutoPrecoCusto = 5m,
                ProdutoQtdEstoque = 1,
                ProdutoEstoqueMinimo = 1,
                ProdutoAtivo = true
            });

            db.SaveChanges();
        }

        _banco.LimparDados();

        using (var db = _banco.CriarContexto())
        {
            Assert.Empty(db.Produtos);
        }
    }
}