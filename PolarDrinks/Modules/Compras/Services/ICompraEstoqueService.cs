using PolarDrinks.Compartilhado.Common;
using PolarDrinks.Modules.Compras.Models;
using PolarDrinks.Modules.Estoque.Models;

namespace PolarDrinks.Modules.Compras.Services
{
    public interface ICompraEstoqueService
    {
        (List<CompraEstoqueModel> Pendentes, List<CompraEstoqueModel> Concluidas) ListarComprasPorStatus();

        List<FornecedorModel> ListarFornecedoresAtivos();
        List<ProdutoModel> ListarTodosProdutos();
        List<ProdutoModel> ListarProdutosAtivosOrdenados();

        ResultadoOperacao CadastrarCompra(
            int? fornecedorId, IEnumerable<(int? ProdutoId, int Quantidade, decimal Preco)> itens, int? usuarioId);

        (CompraEstoqueModel Compra, string? UsuarioConfirmacao)? ObterDetalhes(int id);
        CompraEstoqueModel? ObterParaExcluir(int id);

        ResultadoOperacao ConfirmarEntrega(int id, int? usuarioId);
        ResultadoOperacao ConfirmarExclusao(int id);
    }
}