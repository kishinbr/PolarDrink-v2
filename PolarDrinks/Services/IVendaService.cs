using PolarDrinks.Models;
using PolarDrinks.Services.Common;

namespace PolarDrinks.Services
{
    public interface IVendaService
    {
        List<VendaModel> ListarVendas(DateTime? dataInicio, DateTime? dataFim);
        List<ProdutoModel> ListarProdutosAtivos();

        VendaDetalhesViewModel? ObterDetalhesVenda(int id);
        VendaModel? ObterVendaParaCancelamento(int id);

        ResultadoOperacao FinalizarVenda(VendaModel venda, int? usuarioId);
        ResultadoOperacao CancelarVenda(int id, string? descricao, int? usuarioId);
    }
}