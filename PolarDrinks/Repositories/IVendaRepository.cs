using PolarDrinks.Models;

namespace PolarDrinks.Repositories
{
    public interface IVendaRepository
    {
        List<VendaModel> ObterPorPeriodo(DateTime? dataInicio, DateTime? dataFim);
        VendaModel? ObterPorId(int id);
        (string? Descricao, string? UsuarioNome)? ObterMotivoCancelamento(int vendaId);
        void Adicionar(VendaModel venda);
    }
}