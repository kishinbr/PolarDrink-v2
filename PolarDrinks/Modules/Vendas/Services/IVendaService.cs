using PolarDrinks.Compartilhado.Common;
using PolarDrinks.Modules.Estoque.Models;
using PolarDrinks.Modules.Vendas.Models;

namespace PolarDrinks.Modules.Vendas.Services
{
    public interface IVendaService
    {
        List<VendaModel> ListarVendas(DateTime? dataInicio, DateTime? dataFim);
        List<ProdutoModel> ListarProdutosAtivos();

        (VendaModel Venda, string? MotivoCancelamento, string? UsuarioCancelamento)? ObterDetalhesVenda(int id);
        VendaModel? ObterVendaParaCancelamento(int id);

        ResultadoOperacao FinalizarVenda(FinalizarVendaRequest request, int? usuarioId);
        ResultadoOperacao CancelarVenda(int id, string? descricao, int? usuarioId);
    
    }
}