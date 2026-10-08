using PolarDrinks.Models;
using PolarDrinks.Services.Common;
using PolarDrinks.Models.Requests;

namespace PolarDrinks.Services
{
    public interface IVendaService
    {
        List<VendaModel> ListarVendas(DateTime? dataInicio, DateTime? dataFim);
        List<ProdutoModel> ListarProdutosAtivos();

        VendaDetalhesViewModel? ObterDetalhesVenda(int id);
        VendaModel? ObterVendaParaCancelamento(int id);

        ResultadoOperacao FinalizarVenda(FinalizarVendaRequest request, int? usuarioId);
        ResultadoOperacao CancelarVenda(int id, string? descricao, int? usuarioId);
    }
}