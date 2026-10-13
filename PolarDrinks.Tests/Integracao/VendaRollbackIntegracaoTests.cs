using PolarDrinks.Data;
using PolarDrinks.Models;
using PolarDrinks.Models.Requests;
using PolarDrinks.Repositories;
using PolarDrinks.Services;
using PolarDrinks.Services.Common;

namespace PolarDrinks.Tests.Integracao;

[Collection(ColecaoBancoDeTestes.Nome)]
[Trait("Categoria", "Integracao")]
public class VendaRollbackIntegracaoTests
{
    private readonly BancoDeTestes _banco;

    public VendaRollbackIntegracaoTests(BancoDeTestes banco)
    {
        _banco = banco;
        _banco.LimparDados();
    }

    // CONTROLE: sem falha, tudo é gravado
    [FatoIntegracao]
    public void FinalizarVenda_Valida_DevePersistirVendaItensMovimentacoesEEstoque()
    {
        var (idA, idB) = CriarDoisProdutos();

        using (var db = _banco.CriarContexto())
        {
            var resultado = CriarServico(db, falharNaSegundaSaida: false)
                .FinalizarVenda(CriarRequest(idA, idB), usuarioId: null);

            Assert.True(resultado.Sucesso, resultado.Mensagem);
        }

        // Contexto novo: o que realmente ficou gravado no banco
        using (var db = _banco.CriarContexto())
        {
            Assert.Equal(1, db.Vendas.Count());
            Assert.Equal(2, db.ItensVenda.Count());
            Assert.Equal(2, db.MovimentacoesEstoque.Count());
            Assert.Equal(8, db.Produtos.Single(p => p.ProdutoID == idA).ProdutoQtdEstoque);
            Assert.Equal(8, db.Produtos.Single(p => p.ProdutoID == idB).ProdutoQtdEstoque);
        }
    }

    // FALHA NO MEIO: a venda já foi gravada na transação; o rollback precisa desfazer tudo
    [FatoIntegracao]
    public void FinalizarVenda_FalhaNoMeio_DeveDesfazerTudoNoBanco()
    {
        var (idA, idB) = CriarDoisProdutos();

        using (var db = _banco.CriarContexto())
        {
            var resultado = CriarServico(db, falharNaSegundaSaida: true)
                .FinalizarVenda(CriarRequest(idA, idB), usuarioId: null);

            Assert.False(resultado.Sucesso);
        }

        using (var db = _banco.CriarContexto())
        {
            Assert.Empty(db.Vendas);
            Assert.Empty(db.ItensVenda);
            Assert.Empty(db.MovimentacoesEstoque);
            Assert.Equal(10, db.Produtos.Single(p => p.ProdutoID == idA).ProdutoQtdEstoque);
            Assert.Equal(10, db.Produtos.Single(p => p.ProdutoID == idB).ProdutoQtdEstoque);
        }
    }

    // RETRY NO MESMO CONTEXTO: o Rollback precisa descartar o estado rastreado da tentativa que falhou
    [FatoIntegracao]
    public void FinalizarVenda_DepoisDeUmaFalha_NoMesmoContexto_NaoDeveReaproveitarEstadoDaTentativaAnterior()
    {
        var (idA, idB) = CriarDoisProdutos();

        using (var db = _banco.CriarContexto())
        {
            // 1ª tentativa: falha no meio
            var falha = CriarServico(db, falharNaSegundaSaida: true)
                .FinalizarVenda(CriarRequest(idA, idB), usuarioId: null);
            Assert.False(falha.Sucesso);

            // 2ª tentativa no MESMO contexto (como um retry dentro da mesma requisição)
            var sucesso = CriarServico(db, falharNaSegundaSaida: false)
                .FinalizarVenda(CriarRequest(idA, idB), usuarioId: null);
            Assert.True(sucesso.Sucesso, sucesso.Mensagem);
        }

        using (var db = _banco.CriarContexto())
        {
            Assert.Equal(1, db.Vendas.Count());
            Assert.Equal(2, db.MovimentacoesEstoque.Count());

            // O estoque baixou UMA vez (10 - 2 = 8), não duas
            Assert.Equal(8, db.Produtos.Single(p => p.ProdutoID == idA).ProdutoQtdEstoque);
            Assert.Equal(8, db.Produtos.Single(p => p.ProdutoID == idB).ProdutoQtdEstoque);
        }
    }

    // ---------- Montagem ----------

    // Monta o serviço com as peças REAIS, no mesmo contexto (como a injeção de dependência faz)
    private static VendaService CriarServico(ApplicationDbContext db, bool falharNaSegundaSaida)
    {
        var produtoRepository = new ProdutoRepository(db);
        var movimentacaoRepository = new MovimentacaoEstoqueRepository(db);
        var vendaRepository = new VendaRepository(db);
        var unitOfWork = new UnitOfWork(db);

        IMovimentacaoEstoqueService estoque = new MovimentacaoEstoqueService(produtoRepository, movimentacaoRepository);

        if (falharNaSegundaSaida)
        {
            estoque = new EstoqueQueFalhaNaSegundaSaida(estoque);
        }

        return new VendaService(vendaRepository, produtoRepository, unitOfWork, estoque);
    }

    private (int idA, int idB) CriarDoisProdutos()
    {
        using var db = _banco.CriarContexto();

        var a = NovoProduto("Produto A", "7890000000001");
        var b = NovoProduto("Produto B", "7890000000002");

        db.Produtos.AddRange(a, b);
        db.SaveChanges();

        return (a.ProdutoID, b.ProdutoID);
    }

    private static ProdutoModel NovoProduto(string nome, string codigoBarras)
    {
        return new ProdutoModel
        {
            ProdutoNome = nome,
            ProdutoCodBarra = codigoBarras,
            ProdutoPrecoVenda = 10m,
            ProdutoPrecoCusto = 5m,
            ProdutoQtdEstoque = 10,
            ProdutoEstoqueMinimo = 1,
            ProdutoAtivo = true
        };
    }

    private static FinalizarVendaRequest CriarRequest(int idA, int idB)
    {
        var request = new FinalizarVendaRequest
        {
            VendaTipoPagamento = VendaModel.TipoPagamento.Dinheiro
        };

        request.Itens.Add(new ItemVendaRequest { ProdutoID = idA, ItemVendaQtd = 2 });
        request.Itens.Add(new ItemVendaRequest { ProdutoID = idB, ItemVendaQtd = 2 });

        return request;
    }

    // Estoque "defeituoso" para o teste: delega ao estoque real, mas FALHA na segunda saída
    private sealed class EstoqueQueFalhaNaSegundaSaida : IMovimentacaoEstoqueService
    {
        private readonly IMovimentacaoEstoqueService _real;
        private int _saidas;

        public EstoqueQueFalhaNaSegundaSaida(IMovimentacaoEstoqueService real)
        {
            _real = real;
        }

        public ResultadoOperacao RegistrarSaida(
            int produtoId, int quantidade, string tipo, int? usuarioId,
            int? itemVendaId = null, int? itemPedidoId = null)
        {
            _saidas++;

            if (_saidas == 2)
            {
                throw new InvalidOperationException("Falha simulada na segunda saída de estoque");
            }

            return _real.RegistrarSaida(produtoId, quantidade, tipo, usuarioId, itemVendaId, itemPedidoId);
        }

        public ResultadoOperacao RegistrarDevolucao(
            int produtoId, int quantidade, string tipo, int? usuarioId,
            string? descricao = null, int? itemVendaId = null, int? itemPedidoId = null)
        {
            return _real.RegistrarDevolucao(produtoId, quantidade, tipo, usuarioId, descricao, itemVendaId, itemPedidoId);
        }

        public ResultadoOperacao RegistrarEntrada(int produtoId, int quantidade, int? usuarioId, int itemCompraId)
        {
            return _real.RegistrarEntrada(produtoId, quantidade, usuarioId, itemCompraId);
        }
    }
}