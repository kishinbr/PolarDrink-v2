using PolarDrinks.Models;
using PolarDrinks.Services.Common;
using PolarDrinks.ViewModels;

namespace PolarDrinks.Services
{
    public interface ICompraEstoqueService
    {
        (List<CompraEstoqueModel> Pendentes, List<CompraEstoqueModel> Concluidas) ListarComprasPorStatus();

        List<FornecedorModel> ListarFornecedoresAtivos();
        List<ProdutoModel> ListarTodosProdutos();
        List<ProdutoModel> ListarProdutosAtivosOrdenados();

        void PreencherNomesProdutos(List<ItemCompraCreateVM> itens);

        ResultadoOperacao CadastrarCompra(int? fornecedorId, List<ItemCompraCreateVM> itens, int? usuarioId);

        CompraDetalhesViewModel? ObterDetalhes(int id, bool podeConfirmar);
        CompraDetalhesViewModel? ObterParaExcluir(int id);

        ResultadoOperacao ConfirmarEntrega(int id, int? usuarioId);
        ResultadoOperacao ConfirmarExclusao(int id);
    }
}