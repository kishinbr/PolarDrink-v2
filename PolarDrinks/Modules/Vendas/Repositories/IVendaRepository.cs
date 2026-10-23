using PolarDrinks.Modules.Vendas.Models;

namespace PolarDrinks.Modules.Vendas.Repositories
{
    public interface IVendaRepository
    {
        List<VendaModel> ObterPorPeriodo(DateTime? dataInicio, DateTime? dataFim);
        VendaModel? ObterPorId(int id);
        (string? Descricao, string? UsuarioNome)? ObterMotivoCancelamento(int vendaId);
        void Adicionar(VendaModel venda);

        List<VendaModel> ObterVendasAtivasComDetalhes();
        List<VendaModel> ObterVendasCanceladasComDetalhes();

        (int Quantidade, decimal Faturamento, decimal Lucro) ObterResumoFinanceiro(DateTime? inicio, DateTime? fim);
        ResumoVendasDto ObterResumoPorPeriodo(DateTime inicio, DateTime fim);
    }
}