using NSubstitute;
using PolarDrinks.Compartilhado.Repositories;
using PolarDrinks.Data;
using PolarDrinks.Modules.Estoque.Models;
using PolarDrinks.Modules.Estoque.Repositories;
using PolarDrinks.Modules.Estoque.Services;

namespace PolarDrinks.Tests.Integracao;

[Collection(ColecaoBancoDeTestes.Nome)]
[Trait("Categoria", "Integracao")]
public class CadastroProdutoRollbackIntegracaoTests
{
    private readonly BancoDeTestes _banco;

    public CadastroProdutoRollbackIntegracaoTests(BancoDeTestes banco)
    {
        _banco = banco;
        _banco.LimparDados();
    }

    // CONTROLE: com uma categoria que existe, produto e categoria são gravados
    [FatoIntegracao]
    public void CadastrarProduto_CategoriaValida_DevePersistirProdutoECategoria()
    {
        int categoriaId;

        using (var db = _banco.CriarContexto())
        {
            var categoria = new CategoriaModel { CategoriaNome = "Cervejas" };
            db.Categorias.Add(categoria);
            db.SaveChanges();
            categoriaId = categoria.CategoriaID;
        }

        var armazenamento = CriarArmazenamentoFalso();

        using (var db = _banco.CriarContexto())
        {
            using var conteudo = new MemoryStream(new byte[] { 1, 2, 3 });

            var resultado = CriarServico(db, armazenamento)
                .CadastrarProduto(NovoProduto(), new List<int> { categoriaId }, (conteudo, "foto.jpg"));

            Assert.True(resultado.Sucesso, resultado.Mensagem);
        }

        using (var db = _banco.CriarContexto())
        {
            var produto = db.Produtos.Single();

            Assert.Equal("uploads/produtos/1.jpg", produto.ProdutoImagemUrl);
            Assert.Single(db.ProdutoCategorias.Where(pc =>
                pc.ProdutoID == produto.ProdutoID && pc.CategoriaID == categoriaId));
        }

        armazenamento.DidNotReceive().RemoverImagemProduto(Arg.Any<string?>());
    }

    // FALHA REAL DO BANCO: categoria inexistente viola a chave estrangeira
    [FatoIntegracao]
    public void CadastrarProduto_CategoriaInexistente_NaoDeveDeixarProdutoNemArquivo()
    {
        var armazenamento = CriarArmazenamentoFalso();

        using (var db = _banco.CriarContexto())
        {
            using var conteudo = new MemoryStream(new byte[] { 1, 2, 3 });

            var resultado = CriarServico(db, armazenamento)
                .CadastrarProduto(NovoProduto(), new List<int> { 99999 }, (conteudo, "foto.jpg"));

            Assert.False(resultado.Sucesso);
        }

        // Contexto novo: o que realmente ficou gravado no banco
        using (var db = _banco.CriarContexto())
        {
            Assert.Empty(db.Produtos);
            Assert.Empty(db.ProdutoCategorias);
        }

        // O arquivo já salvo foi removido
        armazenamento.Received(1).RemoverImagemProduto("uploads/produtos/1.jpg");
    }

    // ---------- Montagem ----------

    private static EstoqueService CriarServico(ApplicationDbContext db, IArmazenamentoService armazenamento)
    {
        return new EstoqueService(
            new ProdutoRepository(db),
            new MovimentacaoEstoqueRepository(db),
            armazenamento,
            new UnitOfWork(db));
    }

    // O disco é simulado: devolve um caminho e registra se a remoção foi pedida
    private static IArmazenamentoService CriarArmazenamentoFalso()
    {
        var armazenamento = Substitute.For<IArmazenamentoService>();

        armazenamento
            .SalvarImagemProduto(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns("uploads/produtos/1.jpg");

        return armazenamento;
    }

    private static ProdutoModel NovoProduto()
    {
        return new ProdutoModel
        {
            ProdutoNome = "Vodka Gelada",
            ProdutoCodBarra = "7890000000001",
            ProdutoPrecoVenda = 100m,
            ProdutoPrecoCusto = 40m,
            ProdutoQtdEstoque = 10,
            ProdutoEstoqueMinimo = 2,
            ProdutoAtivo = true
        };
    }
}